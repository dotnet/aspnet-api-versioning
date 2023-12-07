// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Controllers;

using Asp.Versioning.Dispatcher;
using Asp.Versioning.Routing;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using System.Web.Http.Services;
using static Asp.Versioning.ApiVersionMapping;
using static System.Net.HttpStatusCode;
using static System.Reflection.BindingFlags;
using static System.StringComparer;

/// <summary>
/// <para>All caching is in a dedicated cache class, which may be optionally shared across selector instances.</para>
/// <para>Make this a nested class so that nobody else can conflict with our state.</para>
/// <para>Cache is initialized during ctor on a single thread.</para>
/// </summary>
internal sealed class ActionSelectorCacheItem
{
    private static readonly HttpMethod[] cacheListMethodKinds = [HttpMethod.Get, HttpMethod.Put, HttpMethod.Post];
    private readonly HttpControllerDescriptor controllerDescriptor;
    private readonly CandidateAction[] combinedCandidateActions;
    private readonly Dictionary<HttpActionDescriptor, string[]> actionParameterNames = [];
    private readonly ILookup<string, HttpActionDescriptor> combinedActionNameMapping;
    private StandardActionSelectionCache? standardActions;

    internal ActionSelectorCacheItem( HttpControllerDescriptor controllerDescriptor )
    {
        this.controllerDescriptor = controllerDescriptor;

        var validMethods = this.controllerDescriptor.ControllerType
                                                    .GetMethods( Instance | Public )
                                                    .Where( IsValidActionMethod )
                                                    .ToArray();

        combinedCandidateActions = new CandidateAction[validMethods.Length];

        for ( var i = 0; i < validMethods.Length; i++ )
        {
            var actionDescriptor = new ReflectedHttpActionDescriptor( controllerDescriptor, validMethods[i] );

            combinedCandidateActions[i] = new( actionDescriptor );
            actionParameterNames.Add(
                actionDescriptor,
                actionDescriptor.ActionBinding
                                .ParameterBindings
                                .Where( binding => !binding.Descriptor.IsOptional &&
                                                   binding.Descriptor.ParameterType.CanConvertFromString() &&
                                                   binding.WillReadUri() )
                                .Select( binding => binding.Descriptor.Prefix ??
                                                    binding.Descriptor.ParameterName ).ToArray() );
        }

        combinedActionNameMapping =
            combinedCandidateActions
            .Select( c => c.ActionDescriptor )
            .ToLookup( actionDesc => actionDesc.ActionName, OrdinalIgnoreCase );
    }

    internal HttpControllerDescriptor HttpControllerDescriptor => controllerDescriptor;

    private void InitializeStandardActions()
    {
        if ( standardActions != null )
        {
            return;
        }

        var selectionCache = new StandardActionSelectionCache();

        if ( controllerDescriptor.IsAttributeRouted() )
        {
            selectionCache.StandardCandidateActions = [];
        }
        else
        {
            var standardCandidateActions = new List<CandidateAction>( capacity: combinedCandidateActions.Length );

            for ( var i = 0; i < combinedCandidateActions.Length; i++ )
            {
                var candidate = combinedCandidateActions[i];
                var action = (ReflectedHttpActionDescriptor) candidate.ActionDescriptor;

                if ( action.MethodInfo.DeclaringType != controllerDescriptor.ControllerType ||
                     !candidate.ActionDescriptor.IsAttributeRouted() )
                {
                    standardCandidateActions.Add( candidate );
                }
            }

            selectionCache.StandardCandidateActions = [.. standardCandidateActions];
        }

        selectionCache.StandardActionNameMapping =
            selectionCache.StandardCandidateActions
                          .Select( c => c.ActionDescriptor )
                          .ToLookup( actionDesc => actionDesc.ActionName, OrdinalIgnoreCase );

        var len = cacheListMethodKinds.Length;

        selectionCache.CacheListMethods = new CandidateAction[len][];

        for ( var i = 0; i < len; i++ )
        {
            selectionCache.CacheListMethods[i] = FindActionsForMethod( cacheListMethodKinds[i], selectionCache.StandardCandidateActions );
        }

        standardActions = selectionCache;
    }

    internal HttpActionDescriptor? SelectAction(
        HttpControllerContext controllerContext,
        Func<HttpControllerContext, IReadOnlyList<HttpActionDescriptor>, HttpActionDescriptor?> selector )
    {
        ArgumentNullException.ThrowIfNull( selector );
        InitializeStandardActions();

        var firstAttempt = FindAction( controllerContext, selector, ignoreSubRoutes: false );

        if ( firstAttempt.Succeeded )
        {
            return firstAttempt.Action;
        }

        if ( controllerContext.RouteData.GetSubRoutes() == null )
        {
            throw firstAttempt.Exception!;
        }

        var secondAttempt = FindAction( controllerContext, selector, ignoreSubRoutes: true );

        if ( secondAttempt.Succeeded )
        {
            return secondAttempt.Action;
        }

        throw firstAttempt.Exception!;
    }

    private ActionSelectionResult FindAction(
        HttpControllerContext controllerContext,
        Func<HttpControllerContext, IReadOnlyList<HttpActionDescriptor>, HttpActionDescriptor?> selector,
        bool ignoreSubRoutes )
    {
        var selectedCandidates = FindMatchingActions( controllerContext, ignoreSubRoutes );
        if ( selectedCandidates.Count == 0 )
        {
            return new( new HttpResponseException( CreateSelectionError( controllerContext ) ) );
        }

        if ( selector( controllerContext, selectedCandidates ) is CandidateHttpActionDescriptor action )
        {
            ElevateRouteData( controllerContext, action.CandidateAction );
            return new( action );
        }

        if ( selectedCandidates.Count == 1 )
        {
            return new( new HttpResponseException( CreateSelectionError( controllerContext ) ) );
        }

        var ambiguityList = CreateAmbiguousMatchList( selectedCandidates );
        var message = string.Format( CultureInfo.CurrentCulture, SR.ApiControllerActionSelector_AmbiguousMatch, ambiguityList );

        return new( new InvalidOperationException( message ) );
    }

    private static bool IsValidActionMethod( MethodInfo methodInfo )
    {
        if ( methodInfo.IsSpecialName )
        {
            return false;
        }

        if ( methodInfo.GetCustomAttribute<NonActionAttribute>() != null )
        {
            return false;
        }

        var declaringType = methodInfo.GetBaseDefinition().DeclaringType;

        if ( declaringType.IsAssignableFrom( typeof( ApiController ) ) )
        {
            return false;
        }

        var controllerType = typeof( IHttpController );

        if ( !controllerType.IsAssignableFrom( declaringType ) )
        {
            return false;
        }

        var interfaceMap = declaringType.GetInterfaceMap( controllerType );

        if ( interfaceMap.TargetMethods[0] == methodInfo )
        {
            return false;
        }

        return true;
    }

    private static void ElevateRouteData( HttpControllerContext controllerContext, CandidateActionWithParams selectedCandidate ) =>
        controllerContext.RouteData = selectedCandidate.RouteDataSource;

    private IReadOnlyList<CandidateHttpActionDescriptor> FindMatchingActions(
        HttpControllerContext controllerContext,
        bool ignoreSubRoutes = false,
        bool ignoreMethods = false )
    {
        var routeData = controllerContext.RouteData;
        var subRoutes = ignoreSubRoutes ? default : routeData.GetSubRoutes();
        var actionsWithParameters = subRoutes == null ?
            GetInitialCandidateWithParameterListForRegularRoutes( controllerContext, ignoreMethods ) :
            GetInitialCandidateWithParameterListForDirectRoutes( controllerContext, subRoutes, ignoreMethods );

        var actionsFoundByParams = FindActionMatchRequiredRouteAndQueryParameters( actionsWithParameters );
        var orderCandidates = RunOrderFilter( actionsFoundByParams );
        var precedenceCandidates = RunPrecedenceFilter( orderCandidates );
        var selectedCandidates = FindActionMatchMostRouteAndQueryParameters( precedenceCandidates );

        return selectedCandidates.Select( c => new CandidateHttpActionDescriptor( c ) ).ToArray();
    }

    private IEnumerable<HttpMethod> GetAllowedMethods( HttpControllerContext controllerContext )
    {
        var request = controllerContext.Request;
        var apiModel = controllerContext.ControllerDescriptor.GetApiVersionModel();
        var version = apiModel.IsApiVersionNeutral ? ApiVersion.Neutral : request.ApiVersionProperties().RequestedApiVersion!;
        var httpMethods = new HashSet<HttpMethod>();

        for ( var i = 0; i < combinedCandidateActions.Length; i++ )
        {
            var actionDescriptor = combinedCandidateActions[i].ActionDescriptor;
            var endpointModel = actionDescriptor.GetApiVersionMetadata().Map( Explicit );

            if ( endpointModel.IsApiVersionNeutral || endpointModel.ImplementedApiVersions.Contains( version ) )
            {
                httpMethods.AddRange( actionDescriptor.SupportedHttpMethods );
            }
        }

        return httpMethods;
    }

    private HttpResponseMessage CreateSelectionError( HttpControllerContext controllerContext )
    {
        var actionsFoundByParams = FindMatchingActions( controllerContext, ignoreMethods: true );

        if ( actionsFoundByParams.Count == 0 )
        {
            return CreateActionNotFoundResponse( controllerContext );
        }

        var apiModel = controllerContext.ControllerDescriptor.GetApiVersionModel();
        var httpMethods = GetAllowedMethods( controllerContext );
        var exceptionFactory = new HttpResponseExceptionFactory( controllerContext.Request, apiModel );

        return exceptionFactory.CreateMethodNotAllowedResponse( httpMethods );
    }

    private HttpResponseMessage CreateActionNotFoundResponse( HttpControllerContext controllerContext )
    {
        var culture = CultureInfo.CurrentCulture;
        var message = string.Format( culture, SR.ResourceNotFound, controllerContext.Request.RequestUri );
        var messageDetail = string.Format( culture, SR.ApiControllerActionSelector_ActionNotFound, controllerDescriptor.ControllerName );
        return controllerContext.Request.CreateErrorResponse( NotFound, message, messageDetail );
    }

    private static List<CandidateActionWithParams> GetInitialCandidateWithParameterListForDirectRoutes(
        HttpControllerContext controllerContext,
        IEnumerable<IHttpRouteData> subRoutes,
        bool ignoreMethods )
    {
        var candidateActionWithParams = new List<CandidateActionWithParams>();

        if ( subRoutes == null )
        {
            return candidateActionWithParams;
        }

        var request = controllerContext.Request;
        var incomingMethod = controllerContext.Request.Method;
        var queryNameValuePairs = request.GetQueryNameValuePairs();

        foreach ( var subRouteData in subRoutes )
        {
            var combinedParameterNames = GetCombinedParameterNames( queryNameValuePairs, subRouteData.Values );
            var candidates = subRouteData.Route.GetDirectRouteCandidates();

            if ( candidates == null )
            {
                continue;
            }

            subRouteData.Values.TryGetValue( RouteValueKeys.Action, out string? actionName );

            for ( var i = 0; i < candidates.Length; i++ )
            {
                var candidate = candidates[i];

                if ( ( actionName == null || candidate.MatchName( actionName ) ) &&
                     ( ignoreMethods || candidate.MatchVerb( incomingMethod ) ) )
                {
                    candidateActionWithParams.Add( new( candidate, combinedParameterNames, subRouteData ) );
                }
            }
        }

        return candidateActionWithParams;
    }

    private IEnumerable<CandidateActionWithParams> GetInitialCandidateWithParameterListForRegularRoutes(
        HttpControllerContext controllerContext,
        bool ignoreMethods = false )
    {
        var candidates = GetInitialCandidateList( controllerContext, ignoreMethods );
        return GetCandidateActionsWithBindings( controllerContext, candidates );
    }

    private CandidateAction[] GetInitialCandidateList( HttpControllerContext controllerContext, bool ignoreMethods = false )
    {
        var incomingMethod = controllerContext.Request.Method;
        var routeData = controllerContext.RouteData;
        CandidateAction[] candidates;

        if ( routeData.Values.TryGetValue( RouteValueKeys.Action, out string? actionName ) )
        {
            var actionsFoundByName = standardActions!.StandardActionNameMapping![actionName!].ToArray();

            if ( actionsFoundByName.Length == 0 )
            {
                var apiModel = controllerContext.ControllerDescriptor.GetApiVersionModel();
                var exceptionFactory = new HttpResponseExceptionFactory( controllerContext.Request, apiModel );
                var httpMethods = GetAllowedMethods( controllerContext );

                throw exceptionFactory.NewMethodNotAllowedException( httpMethods );
            }

            var candidatesFoundByName = new CandidateAction[actionsFoundByName.Length];

            for ( var i = 0; i < actionsFoundByName.Length; i++ )
            {
                candidatesFoundByName[i] = new( actionsFoundByName[i] );
            }

            candidates = ignoreMethods ? candidatesFoundByName : FilterIncompatibleMethods( incomingMethod, candidatesFoundByName );
        }
        else
        {
            candidates = ignoreMethods
                ? standardActions!.StandardCandidateActions!
                : FindActionsForMethod( incomingMethod, standardActions!.CacheListMethods!, standardActions!.StandardCandidateActions! );
        }

        return candidates;
    }

    private static CandidateAction[] FilterIncompatibleMethods( HttpMethod incomingMethod, CandidateAction[] candidatesFoundByName ) =>
        candidatesFoundByName.Where( c => c.ActionDescriptor.SupportedHttpMethods.Contains( incomingMethod ) ).ToArray();

    internal ILookup<string, HttpActionDescriptor> GetActionMapping() => combinedActionNameMapping;

    private static ISet<string> GetCombinedParameterNames( IEnumerable<KeyValuePair<string, string>> queryNameValuePairs, IDictionary<string, object> routeValues )
    {
        var routeParameterNames = new HashSet<string>( routeValues.Keys, OrdinalIgnoreCase );

        routeParameterNames.Remove( RouteValueKeys.Controller );
        routeParameterNames.Remove( RouteValueKeys.Action );

        var combinedParameterNames = new HashSet<string>( routeParameterNames, OrdinalIgnoreCase );

        if ( queryNameValuePairs != null )
        {
            foreach ( var queryNameValuePair in queryNameValuePairs )
            {
                combinedParameterNames.Add( queryNameValuePair.Key );
            }
        }

        return combinedParameterNames;
    }

    private List<CandidateActionWithParams> FindActionMatchRequiredRouteAndQueryParameters( IEnumerable<CandidateActionWithParams> candidatesFound )
    {
        var matches = new List<CandidateActionWithParams>();

        foreach ( var candidate in candidatesFound )
        {
            var descriptor = candidate.ActionDescriptor;
            var candidateControllerDescriptor = Decorator.GetInner( descriptor.ControllerDescriptor );

            if ( candidateControllerDescriptor == controllerDescriptor &&
                 IsSubset( actionParameterNames[descriptor], candidate.CombinedParameterNames ) )
            {
                matches.Add( candidate );
            }
        }

        return matches;
    }

    private List<CandidateActionWithParams> FindActionMatchMostRouteAndQueryParameters( List<CandidateActionWithParams> candidatesFound ) =>
        candidatesFound.Count < 2 ? candidatesFound : [.. candidatesFound.GroupBy( c => actionParameterNames[c.ActionDescriptor].Length ).OrderByDescending( g => g.Key ).First()];

    private static CandidateActionWithParams[] GetCandidateActionsWithBindings( HttpControllerContext controllerContext, CandidateAction[] candidatesFound )
    {
        var request = controllerContext.Request;
        var queryNameValuePairs = request.GetQueryNameValuePairs();
        var routeData = controllerContext.RouteData;
        var routeValues = routeData.Values;
        var combinedParameterNames = GetCombinedParameterNames( queryNameValuePairs, routeValues );
        var candidatesWithParams = new CandidateActionWithParams[candidatesFound.Length];

        for ( var i = 0; i < candidatesFound.Length; i++ )
        {
            candidatesWithParams[i] = new( candidatesFound[i], combinedParameterNames, routeData );
        }

        return candidatesWithParams;
    }

    private static bool IsSubset( string[] actionParameters, ISet<string> routeAndQueryParameters )
    {
        for ( var i = 0; i < actionParameters.Length; i++ )
        {
            if ( !routeAndQueryParameters.Contains( actionParameters[i] ) )
            {
                return false;
            }
        }

        return true;
    }

    private static List<CandidateActionWithParams> RunOrderFilter( List<CandidateActionWithParams> candidatesFound )
    {
        if ( candidatesFound.Count == 0 )
        {
            return candidatesFound;
        }

        var minOrder = candidatesFound.Min( c => c.CandidateAction.Order );

        return candidatesFound.Where( c => c.CandidateAction.Order == minOrder ).AsList();
    }

    private static List<CandidateActionWithParams> RunPrecedenceFilter( List<CandidateActionWithParams> candidatesFound )
    {
        if ( candidatesFound.Count == 0 )
        {
            return candidatesFound;
        }

        var highestPrecedence = candidatesFound.Min( c => c.CandidateAction.Precedence );

        return candidatesFound.Where( c => c.CandidateAction.Precedence == highestPrecedence ).AsList();
    }

    private static CandidateAction[] FindActionsForMethod( HttpMethod method, CandidateAction[][] actionsByMethod, CandidateAction[] otherActions )
    {
        for ( var i = 0; i < cacheListMethodKinds.Length; i++ )
        {
            if ( ReferenceEquals( method, cacheListMethodKinds[i] ) )
            {
                return actionsByMethod[i];
            }
        }

        return FindActionsForMethod( method, otherActions );
    }

    private static CandidateAction[] FindActionsForMethod( HttpMethod method, CandidateAction[] candidates )
    {
        var listCandidates = new List<CandidateAction>();
        FindActionsForMethod( method, candidates, listCandidates );
        return [.. listCandidates];
    }

    private static void FindActionsForMethod( HttpMethod method, CandidateAction[] candidates, List<CandidateAction> listCandidates )
    {
        for ( var i = 0; i < candidates.Length; i++ )
        {
            var candidate = candidates[i];
            var action = candidate.ActionDescriptor;

            if ( action != null && action.SupportedHttpMethods.Contains( method ) )
            {
                listCandidates.Add( candidate );
            }
        }
    }

    internal static string CreateAmbiguousMatchList( IEnumerable<HttpActionDescriptor> ambiguousCandidates )
    {
        var exceptionMessageBuilder = new StringBuilder();

        foreach ( var descriptor in ambiguousCandidates )
        {
            var controllerDescriptor = descriptor.ControllerDescriptor;
            var controllerTypeName = controllerDescriptor != null && controllerDescriptor.ControllerType != null
                ? controllerDescriptor.ControllerType.FullName
                : string.Empty;

            exceptionMessageBuilder.AppendLine();
            exceptionMessageBuilder.AppendFormat(
                CultureInfo.CurrentCulture,
                SR.ActionSelector_AmbiguousMatchType,
                descriptor.ActionName,
                controllerTypeName );
        }

        return exceptionMessageBuilder.ToString();
    }
}