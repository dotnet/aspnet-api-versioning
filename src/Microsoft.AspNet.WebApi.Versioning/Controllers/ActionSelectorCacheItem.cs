namespace Microsoft.Web.Http.Controllers
{
    using Microsoft.Web.Http.Dispatcher;
    using Microsoft.Web.Http.Routing;
    using Microsoft.Web.Http.Versioning;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Reflection;
    using System.Text;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Routing;
    using System.Web.Http.Services;
    using static System.Net.HttpStatusCode;
    using static System.StringComparer;

    /// <content>
    /// Provides additional content for the <see cref="ApiVersionActionSelector"/> class.
    /// </content>
    public partial class ApiVersionActionSelector
    {
        /// <summary>
        /// <para>All caching is in a dedicated cache class, which may be optionally shared across selector instances.</para>
        /// <para>Make this a nested class so that nobody else can conflict with our state.</para>
        /// <para>Cache is initialized during ctor on a single thread.</para>
        /// </summary>
        sealed class ActionSelectorCacheItem
        {
            static readonly HttpMethod[] cacheListVerbKinds = new[] { HttpMethod.Get, HttpMethod.Put, HttpMethod.Post };
            static readonly Type ApiControllerType = typeof( ApiController );
            readonly HttpControllerDescriptor controllerDescriptor;
            readonly CandidateAction[] combinedCandidateActions;
            readonly IDictionary<HttpActionDescriptor, string[]> actionParameterNames = new Dictionary<HttpActionDescriptor, string[]>();
            readonly ILookup<string, HttpActionDescriptor> combinedActionNameMapping;
            readonly HashSet<HttpMethod> allowedMethods = new HashSet<HttpMethod>();
            StandardActionSelectionCache? standardActions;

            internal ActionSelectorCacheItem( HttpControllerDescriptor controllerDescriptor )
            {
                this.controllerDescriptor = controllerDescriptor;

                var allMethods = this.controllerDescriptor.ControllerType.GetMethods( BindingFlags.Instance | BindingFlags.Public );
                var validMethods = Array.FindAll( allMethods, IsValidActionMethod );

                combinedCandidateActions = new CandidateAction[validMethods.Length];

                for ( var i = 0; i < validMethods.Length; i++ )
                {
                    var method = validMethods[i];
                    var actionDescriptor = new ReflectedHttpActionDescriptor( controllerDescriptor, method );
                    var actionBinding = actionDescriptor.ActionBinding;

                    allowedMethods.AddRange( actionDescriptor.SupportedHttpMethods );
                    combinedCandidateActions[i] = new CandidateAction( actionDescriptor );

                    actionParameterNames.Add(
                        actionDescriptor,
                        actionBinding.ParameterBindings
                            .Where( binding => !binding.Descriptor.IsOptional && binding.Descriptor.ParameterType.CanConvertFromString() && binding.WillReadUri() )
                            .Select( binding => binding.Descriptor.Prefix ?? binding.Descriptor.ParameterName ).ToArray() );
                }

                combinedActionNameMapping =
                    combinedCandidateActions
                    .Select( c => c.ActionDescriptor )
                    .ToLookup( actionDesc => actionDesc.ActionName, OrdinalIgnoreCase );
            }

            internal HttpControllerDescriptor HttpControllerDescriptor => controllerDescriptor;

            void InitializeStandardActions()
            {
                if ( standardActions != null )
                {
                    return;
                }

                var selectionCache = new StandardActionSelectionCache();

                if ( controllerDescriptor.IsAttributeRouted() )
                {
                    selectionCache.StandardCandidateActions = new CandidateAction[0];
                }
                else
                {
                    var standardCandidateActions = new List<CandidateAction>();

                    for ( var i = 0; i < combinedCandidateActions.Length; i++ )
                    {
                        var candidate = combinedCandidateActions[i];
                        var action = (ReflectedHttpActionDescriptor) candidate.ActionDescriptor;

                        if ( action.MethodInfo.DeclaringType != controllerDescriptor.ControllerType || !candidate.ActionDescriptor.IsAttributeRouted() )
                        {
                            standardCandidateActions.Add( candidate );
                        }
                    }

                    selectionCache.StandardCandidateActions = standardCandidateActions.ToArray();
                }

                selectionCache.StandardActionNameMapping = selectionCache.StandardCandidateActions.Select( c => c.ActionDescriptor ).ToLookup( actionDesc => actionDesc.ActionName, OrdinalIgnoreCase );

                var len = cacheListVerbKinds.Length;

                selectionCache.CacheListVerbs = new CandidateAction[len][];

                for ( var i = 0; i < len; i++ )
                {
                    selectionCache.CacheListVerbs[i] = FindActionsForVerbWorker( cacheListVerbKinds[i], selectionCache.StandardCandidateActions );
                }

                standardActions = selectionCache;
            }

            internal HttpActionDescriptor? SelectAction( HttpControllerContext controllerContext, Func<HttpControllerContext, IReadOnlyList<HttpActionDescriptor>, HttpActionDescriptor?> selector )
            {
                if ( selector == null )
                {
                    throw new ArgumentNullException( nameof( selector ) );
                }

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

            ActionSelectionResult FindAction( HttpControllerContext controllerContext, Func<HttpControllerContext, IReadOnlyList<HttpActionDescriptor>, HttpActionDescriptor?> selector, bool ignoreSubRoutes )
            {
                var selectedCandidates = FindMatchingActions( controllerContext, ignoreSubRoutes );

                if ( selectedCandidates.Count == 0 )
                {
#pragma warning disable CA2000 // Dispose objects before losing scope
                    return new ActionSelectionResult( new HttpResponseException( CreateSelectionError( controllerContext ) ) );
#pragma warning restore CA2000 // Dispose objects before losing scope
                }

                if ( selector( controllerContext, selectedCandidates ) is CandidateHttpActionDescriptor action )
                {
                    ElevateRouteData( controllerContext, action.CandidateAction );
                    return new ActionSelectionResult( action );
                }

                if ( selectedCandidates.Count == 1 )
                {
#pragma warning disable CA2000 // Dispose objects before losing scope
                    return new ActionSelectionResult( new HttpResponseException( CreateSelectionError( controllerContext ) ) );
#pragma warning restore CA2000 // Dispose objects before losing scope
                }

                var ambiguityList = CreateAmbiguousMatchList( selectedCandidates );

                return new ActionSelectionResult( new InvalidOperationException( SR.ApiControllerActionSelector_AmbiguousMatch.FormatDefault( ambiguityList ) ) );
            }

            static void ElevateRouteData( HttpControllerContext controllerContext, CandidateActionWithParams selectedCandidate ) => controllerContext.RouteData = selectedCandidate.RouteDataSource;

            IReadOnlyList<CandidateHttpActionDescriptor> FindMatchingActions( HttpControllerContext controllerContext, bool ignoreSubRoutes = false, bool ignoreVerbs = false )
            {
                var routeData = controllerContext.RouteData;
                var subRoutes = ignoreSubRoutes ? default : routeData.GetSubRoutes();
                var actionsWithParameters = subRoutes == null ?
                    GetInitialCandidateWithParameterListForRegularRoutes( controllerContext, ignoreVerbs ) :
                    GetInitialCandidateWithParameterListForDirectRoutes( controllerContext, subRoutes, ignoreVerbs );

                var actionsFoundByParams = FindActionMatchRequiredRouteAndQueryParameters( actionsWithParameters );
                var orderCandidates = RunOrderFilter( actionsFoundByParams );
                var precedenceCandidates = RunPrecedenceFilter( orderCandidates );
                var selectedCandidates = FindActionMatchMostRouteAndQueryParameters( precedenceCandidates );

                return selectedCandidates.Select( c => new CandidateHttpActionDescriptor( c ) ).ToArray();
            }

            HttpResponseMessage CreateSelectionError( HttpControllerContext controllerContext )
            {
                var actionsFoundByParams = FindMatchingActions( controllerContext, ignoreVerbs: true );

                if ( actionsFoundByParams.Count == 0 )
                {
                    return CreateActionNotFoundResponse( controllerContext );
                }

                var request = controllerContext.Request;
                var model = controllerContext.ControllerDescriptor.GetApiVersionModel();
                var exceptionFactory = new HttpResponseExceptionFactory( request, new Lazy<ApiVersionModel>( () => model ) );

                return exceptionFactory.CreateMethodNotAllowedResponse( model.IsApiVersionNeutral, allowedMethods );
            }

            HttpResponseMessage CreateActionNotFoundResponse( HttpControllerContext controllerContext )
            {
                var message = SR.ResourceNotFound.FormatDefault( controllerContext.Request.RequestUri );
                var messageDetail = SR.ApiControllerActionSelector_ActionNotFound.FormatDefault( controllerDescriptor.ControllerName );
                return controllerContext.Request.CreateErrorResponse( NotFound, message, messageDetail );
            }

            static List<CandidateActionWithParams> GetInitialCandidateWithParameterListForDirectRoutes( HttpControllerContext controllerContext, IEnumerable<IHttpRouteData> subRoutes, bool ignoreVerbs )
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

                    subRouteData.Values.TryGetValue( RouteValueKeys.Action, out string actionName );

                    foreach ( var candidate in candidates )
                    {
                        if ( actionName == null || candidate.MatchName( actionName ) )
                        {
                            if ( ignoreVerbs || candidate.MatchVerb( incomingMethod ) )
                            {
                                candidateActionWithParams.Add( new CandidateActionWithParams( candidate, combinedParameterNames, subRouteData ) );
                            }
                        }
                    }
                }

                return candidateActionWithParams;
            }

            IEnumerable<CandidateActionWithParams> GetInitialCandidateWithParameterListForRegularRoutes( HttpControllerContext controllerContext, bool ignoreVerbs = false )
            {
                var candidates = GetInitialCandidateList( controllerContext, ignoreVerbs );
                return GetCandidateActionsWithBindings( controllerContext, candidates );
            }

            CandidateAction[] GetInitialCandidateList( HttpControllerContext controllerContext, bool ignoreVerbs = false )
            {
                var incomingMethod = controllerContext.Request.Method;
                var routeData = controllerContext.RouteData;
                CandidateAction[] candidates;

                if ( routeData.Values.TryGetValue( RouteValueKeys.Action, out string actionName ) )
                {
                    var actionsFoundByName = standardActions!.StandardActionNameMapping![actionName].ToArray();

                    if ( actionsFoundByName.Length == 0 )
                    {
                        var request = controllerContext.Request;
                        var model = controllerContext.ControllerDescriptor.GetApiVersionModel();
                        var exceptionFactory = new HttpResponseExceptionFactory( request, new Lazy<ApiVersionModel>( () => model ) );

                        throw exceptionFactory.NewMethodNotAllowedException( model.IsApiVersionNeutral, allowedMethods );
                    }

                    var candidatesFoundByName = new CandidateAction[actionsFoundByName.Length];

                    for ( var i = 0; i < actionsFoundByName.Length; i++ )
                    {
                        candidatesFoundByName[i] = new CandidateAction( actionsFoundByName[i] );
                    }

                    if ( ignoreVerbs )
                    {
                        candidates = candidatesFoundByName;
                    }
                    else
                    {
                        candidates = FilterIncompatibleVerbs( incomingMethod, candidatesFoundByName );
                    }
                }
                else
                {
                    if ( ignoreVerbs )
                    {
                        candidates = standardActions!.StandardCandidateActions!;
                    }
                    else
                    {
                        candidates = FindActionsForVerb( incomingMethod, standardActions!.CacheListVerbs!, standardActions!.StandardCandidateActions! );
                    }
                }

                return candidates;
            }

            static CandidateAction[] FilterIncompatibleVerbs( HttpMethod incomingMethod, CandidateAction[] candidatesFoundByName ) =>
                candidatesFoundByName.Where( c => c.ActionDescriptor.SupportedHttpMethods.Contains( incomingMethod ) ).ToArray();

            internal ILookup<string, HttpActionDescriptor> GetActionMapping() => combinedActionNameMapping;

            static ISet<string> GetCombinedParameterNames( IEnumerable<KeyValuePair<string, string>> queryNameValuePairs, IDictionary<string, object> routeValues )
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

            List<CandidateActionWithParams> FindActionMatchRequiredRouteAndQueryParameters( IEnumerable<CandidateActionWithParams> candidatesFound )
            {
                var matches = new List<CandidateActionWithParams>();

                foreach ( var candidate in candidatesFound )
                {
                    var descriptor = candidate.ActionDescriptor;
                    var candidateControllerDescriptor = Decorator.GetInner( descriptor.ControllerDescriptor );

                    if ( candidateControllerDescriptor == controllerDescriptor && IsSubset( actionParameterNames[descriptor], candidate.CombinedParameterNames ) )
                    {
                        matches.Add( candidate );
                    }
                }

                return matches;
            }

            List<CandidateActionWithParams> FindActionMatchMostRouteAndQueryParameters( List<CandidateActionWithParams> candidatesFound ) =>
                candidatesFound.Count < 2 ? candidatesFound : candidatesFound.GroupBy( c => actionParameterNames[c.ActionDescriptor].Length ).OrderByDescending( g => g.Key ).First().ToList();

            static CandidateActionWithParams[] GetCandidateActionsWithBindings( HttpControllerContext controllerContext, CandidateAction[] candidatesFound )
            {
                var request = controllerContext.Request;
                var queryNameValuePairs = request.GetQueryNameValuePairs();
                var routeData = controllerContext.RouteData;
                var routeValues = routeData.Values;
                var combinedParameterNames = GetCombinedParameterNames( queryNameValuePairs, routeValues );
                var candidatesWithParams = Array.ConvertAll( candidatesFound, candidate => new CandidateActionWithParams( candidate, combinedParameterNames, routeData ) );

                return candidatesWithParams;
            }

            static bool IsSubset( string[] actionParameters, ISet<string> routeAndQueryParameters )
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

            static List<CandidateActionWithParams> RunOrderFilter( List<CandidateActionWithParams> candidatesFound )
            {
                if ( candidatesFound.Count == 0 )
                {
                    return candidatesFound;
                }

                var minOrder = candidatesFound.Min( c => c.CandidateAction.Order );

                return candidatesFound.Where( c => c.CandidateAction.Order == minOrder ).AsList();
            }

            static List<CandidateActionWithParams> RunPrecedenceFilter( List<CandidateActionWithParams> candidatesFound )
            {
                if ( candidatesFound.Count == 0 )
                {
                    return candidatesFound;
                }

                var highestPrecedence = candidatesFound.Min( c => c.CandidateAction.Precedence );

                return candidatesFound.Where( c => c.CandidateAction.Precedence == highestPrecedence ).AsList();
            }

            static CandidateAction[] FindActionsForVerb( HttpMethod verb, CandidateAction[][] actionsByVerb, CandidateAction[] otherActions )
            {
                for ( var i = 0; i < cacheListVerbKinds.Length; i++ )
                {
                    if ( ReferenceEquals( verb, cacheListVerbKinds[i] ) )
                    {
                        return actionsByVerb[i];
                    }
                }

                return FindActionsForVerbWorker( verb, otherActions );
            }

            static CandidateAction[] FindActionsForVerbWorker( HttpMethod verb, CandidateAction[] candidates )
            {
                var listCandidates = new List<CandidateAction>();
                FindActionsForVerbWorker( verb, candidates, listCandidates );
                return listCandidates.ToArray();
            }

            static void FindActionsForVerbWorker( HttpMethod verb, CandidateAction[] candidates, List<CandidateAction> listCandidates )
            {
                for ( var i = 0; i < candidates.Length; i++ )
                {
                    var candidate = candidates[i];
                    var action = candidate.ActionDescriptor;

                    if ( action != null && action.SupportedHttpMethods.Contains( verb ) )
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
                    var controllerTypeName = default( string );

                    if ( controllerDescriptor != null && controllerDescriptor.ControllerType != null )
                    {
                        controllerTypeName = controllerDescriptor.ControllerType.FullName;
                    }
                    else
                    {
                        controllerTypeName = string.Empty;
                    }

                    exceptionMessageBuilder.AppendLine();
                    exceptionMessageBuilder.Append( SR.ActionSelector_AmbiguousMatchType.FormatDefault( descriptor.ActionName, controllerTypeName ) );
                }

                return exceptionMessageBuilder.ToString();
            }

            static bool IsValidActionMethod( MethodInfo methodInfo )
            {
                if ( methodInfo.IsSpecialName )
                {
                    return false;
                }

                if ( methodInfo.GetBaseDefinition().DeclaringType.IsAssignableFrom( ApiControllerType ) )
                {
                    return false;
                }

                if ( methodInfo.GetCustomAttribute<NonActionAttribute>() != null )
                {
                    return false;
                }

                return true;
            }
        }
    }
}