namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Extensions;
    using Microsoft.AspNet.OData.Routing;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.ActionConstraints;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.OData.Edm;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using static System.Linq.Expressions.Expression;

    /// <summary>
    /// Represents the logic for selecting an API-versioned, action method with additional support for OData actions.
    /// </summary>
    [CLSCompliant( false )]
    public class ODataApiVersionActionSelector : ApiVersionActionSelector
    {
        static readonly Func<object, IReadOnlyList<IEdmOptionalParameter>> getOptionalParameters = NewGetOptionalParametersFunc();
        readonly IModelBinderFactory modelBinderFactory;
        readonly IModelMetadataProvider modelMetadataProvider;
        readonly IOptions<MvcOptions> mvcOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataApiVersionActionSelector"/> class.
        /// </summary>
        /// <param name="actionDescriptorCollectionProvider">The <see cref="IActionDescriptorCollectionProvider "/> used to select candidate routes.</param>
        /// <param name="actionConstraintProviders">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IActionConstraintProvider">action constraint providers</see>.</param>
        /// <param name="options">The <see cref="ApiVersioningOptions">options</see> associated with the action selector.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        /// <param name="routePolicy">The <see cref="IApiVersionRoutePolicy">route policy</see> applied to candidate matches.</param>
        /// <param name="modelBinderFactory">The <see cref="IModelBinderFactory"/> used to create model binders.</param>
        /// <param name="modelMetadataProvider">The <see cref="IModelMetadataProvider"/> used to resolve model metadata.</param>
        /// <param name="mvcOptions">The <see cref="MvcOptions">MVC options</see> associated with the action selector.</param>
        public ODataApiVersionActionSelector(
            IActionDescriptorCollectionProvider actionDescriptorCollectionProvider,
            IEnumerable<IActionConstraintProvider> actionConstraintProviders,
            IOptions<ApiVersioningOptions> options,
            ILoggerFactory loggerFactory,
            IApiVersionRoutePolicy routePolicy,
            IModelBinderFactory modelBinderFactory,
            IModelMetadataProvider modelMetadataProvider,
            IOptions<MvcOptions> mvcOptions )
            : base( actionDescriptorCollectionProvider, actionConstraintProviders, options, loggerFactory, routePolicy )
        {
            this.modelBinderFactory = modelBinderFactory;
            this.modelMetadataProvider = modelMetadataProvider;
            this.mvcOptions = mvcOptions;
        }

        /// <summary>
        /// Gets a value indicating whether endpoint routing is enabled.
        /// </summary>
        /// <value>True if endpoint routing is enabled; otherwise, false.</value>
        protected bool UsingEndpointRouting => mvcOptions.Value.EnableEndpointRouting;

        /// <inheritdoc />
        public override IReadOnlyList<ActionDescriptor> SelectCandidates( RouteContext context )
        {
            if ( context == null )
            {
                throw new ArgumentNullException( nameof( context ) );
            }

            var httpContext = context.HttpContext;
            var feature = httpContext.ODataFeature();
            var odataPath = feature.Path;
            var routeValues = context.RouteData.Values;
            var notODataCandidate = odataPath == null || routeValues.ContainsKey( ODataRouteConstants.Action );

            if ( notODataCandidate )
            {
                return base.SelectCandidates( context );
            }

            var routingConventions = httpContext.Request.GetRoutingConventions();

            if ( routingConventions == null )
            {
                return base.SelectCandidates( context );
            }

            var bestCandidates = new List<ActionDescriptor>();
            var actionNames = new HashSet<string>( StringComparer.OrdinalIgnoreCase );
            var container = httpContext.RequestServices.GetRequiredService<IPerRouteContainer>();
            var routePrefix = container.GetRoutePrefix( feature.RouteName );
            var comparer = StringComparer.OrdinalIgnoreCase;

            foreach ( var convention in routingConventions )
            {
                var candidates = convention.SelectAction( context );

                if ( candidates != null )
                {
                    foreach ( var candidate in candidates )
                    {
                        if ( !( candidate.AttributeRouteInfo is ODataAttributeRouteInfo info ) ||
                             !comparer.Equals( routePrefix, info.RoutePrefix ) )
                        {
                            continue;
                        }

                        actionNames.Add( candidate.ActionName );
                        bestCandidates.Add( candidate );
                    }
                }
            }

            if ( bestCandidates.Count == 0 )
            {
                return base.SelectCandidates( context );
            }

            if ( !routeValues.ContainsKey( ODataRouteConstants.Action ) && actionNames.Count == 1 )
            {
                routeValues[ODataRouteConstants.Action] = actionNames.Single();
            }

            return bestCandidates;
        }

        /// <inheritdoc />
        public override ActionDescriptor? SelectBestCandidate( RouteContext context, IReadOnlyList<ActionDescriptor> candidates )
        {
            if ( context == null )
            {
                throw new ArgumentNullException( nameof( context ) );
            }

            if ( candidates == null )
            {
                throw new ArgumentNullException( nameof( candidates ) );
            }

            var httpContext = context.HttpContext;
            var odata = httpContext.ODataFeature();
            var odataRouteCandidate = odata.Path != null;

            if ( !odataRouteCandidate )
            {
                return base.SelectBestCandidate( context, candidates );
            }

            if ( IsRequestedApiVersionAmbiguous( context, out var apiVersion ) )
            {
                return null;
            }

            var matches = EvaluateActionConstraints( context, candidates );
            var selectionContext = new ActionSelectionContext( httpContext, matches, apiVersion );
            var bestActions = SelectBestActions( selectionContext );
            var finalMatches = Array.Empty<ActionDescriptor>();

            if ( bestActions.Count > 0 )
            {
                // REF: https://github.com/OData/WebApi/blob/master/src/Microsoft.AspNetCore.OData/Routing/ODataActionSelector.cs
                var routeValues = context.RouteData.Values;
                var conventionsStore = odata.RoutingConventionsStore ?? new Dictionary<string, object>( capacity: 0 );
                var availableKeys = new HashSet<string>( routeValues.Keys.Where( IsAvailableKey ), StringComparer.OrdinalIgnoreCase );
                var availableKeysCount = conventionsStore.TryGetValue( ODataRouteConstants.KeyCount, out var v ) ? (int) v : 0;
                var possibleCandidates = bestActions.Select( candidate => new ActionIdAndParameters( candidate, ParameterHasRegisteredModelBinder ) );
                var optionalParameters = routeValues.TryGetValue( ODataRouteConstants.OptionalParameters, out var wrapper ) ?
                                         GetOptionalParameters( wrapper ) :
                                         Array.Empty<IEdmOptionalParameter>();
                var matchedCandidates = possibleCandidates
                    .Where( c => TryMatch( httpContext, c, availableKeys, conventionsStore, optionalParameters, availableKeysCount ) )
                    .OrderByDescending( c => c.FilteredParameters.Count )
                    .ThenByDescending( c => c.TotalParameterCount )
                    .ToArray();

                if ( matchedCandidates.Length == 1 )
                {
                    finalMatches = new[] { matchedCandidates[0].Action };
                }
                else if ( matchedCandidates.Length > 1 )
                {
                    var results = matchedCandidates.Where( c => ActionAcceptsMethod( c.Action, httpContext.Request.Method ) ).ToArray();

                    finalMatches = results.Length switch
                    {
                        0 => matchedCandidates.Where( c => c.FilteredParameters.Count == availableKeysCount ).Select( c => c.Action ).ToArray(),
                        1 => new[] { results[0].Action },
                        _ => results.Where( c => c.FilteredParameters.Count == availableKeysCount ).Select( c => c.Action ).ToArray(),
                    };
                }
            }

            var feature = httpContext.Features.Get<IApiVersioningFeature>();
            var selectionResult = feature.SelectionResult;

            feature.RequestedApiVersion = selectionContext.RequestedVersion;
            selectionResult.AddCandidates( candidates );

            if ( finalMatches.Length > 0 )
            {
                selectionResult.AddMatches( finalMatches );
            }
            else
            {
                // OData endpoint routing calls back through IActionSelector. if endpoint routing is enabled
                // then the answer is final; proceed to route policy. if classic routing, it's possible the
                // IActionSelector will be entered again
                if ( !UsingEndpointRouting )
                {
                    return null;
                }
            }

            return RoutePolicy.Evaluate( context, selectionResult );
        }

        static bool IsRouteParameter( string key )
        {
            if ( string.IsNullOrEmpty( key ) )
            {
                return false;
            }

            return key[0] == '{' && key.Length > 1 && key[^1] == '}';
        }

        static bool IsAvailableKey( string key ) => !IsRouteParameter( key ) && key != ODataRouteConstants.Action && key != ODataRouteConstants.ODataPath;

        static bool RequestHasBody( HttpContext context )
        {
            var method = context.Request.Method.ToUpperInvariant();

            switch ( method )
            {
                case "POST":
                case "PUT":
                case "PATCH":
                case "MERGE":
                    return true;
            }

            return false;
        }

        static bool ActionAcceptsMethod( ActionDescriptor action, string method ) =>
            action.GetHttpMethods().Contains( method, StringComparer.OrdinalIgnoreCase );

        static Func<object, IReadOnlyList<IEdmOptionalParameter>> NewGetOptionalParametersFunc()
        {
            var type = Type.GetType( "Microsoft.AspNet.OData.Routing.ODataOptionalParameter, Microsoft.AspNetCore.OData", throwOnError: true, ignoreCase: false );
            var p = Parameter( typeof( object ) );
            var body = Property( Convert( p, type ), "OptionalParameters" );
            var lambda = Lambda<Func<object, IReadOnlyList<IEdmOptionalParameter>>>( body, p );

            return lambda.Compile();
        }

        static IReadOnlyList<IEdmOptionalParameter> GetOptionalParameters( object value )
        {
            if ( value is null )
            {
                return Array.Empty<IEdmOptionalParameter>();
            }

            return getOptionalParameters( value );
        }

        bool TryMatch(
           HttpContext context,
           ActionIdAndParameters action,
           ISet<string> availableKeys,
           IDictionary<string, object> conventionsStore,
           IReadOnlyList<IEdmOptionalParameter> optionalParameters,
           int availableKeysCount )
        {
            var parameters = action.FilteredParameters;
            var totalParameterCount = action.TotalParameterCount;

            if ( availableKeys.Contains( ODataRouteConstants.NavigationProperty ) )
            {
                availableKeysCount -= 1;
            }

            if ( totalParameterCount < availableKeysCount )
            {
                return false;
            }

            var matchedBody = false;
            var keys = conventionsStore.Keys.ToArray();

            for ( var i = 0; i < parameters.Count; i++ )
            {
                var parameter = parameters[i];
                var parameterName = parameter.Name;

                if ( availableKeys.Contains( parameterName ) )
                {
                    continue;
                }

                var matchesKey = false;

                for ( var j = 0; j < keys.Length; j++ )
                {
                    if ( keys[j].Contains( parameterName, StringComparison.Ordinal ) )
                    {
                        matchesKey = true;
                        break;
                    }
                }

                if ( matchesKey )
                {
                    continue;
                }

                if ( context.Request.Query.ContainsKey( parameterName ) )
                {
                    continue;
                }

                if ( parameter is ControllerParameterDescriptor param && optionalParameters.Count > 0 )
                {
                    if ( param.ParameterInfo.IsOptional && optionalParameters.Any( p => string.Equals( p.Name, parameterName, StringComparison.OrdinalIgnoreCase ) ) )
                    {
                        continue;
                    }
                }

                if ( ParameterHasRegisteredModelBinder( parameter ) )
                {
                    continue;
                }

                if ( !matchedBody && RequestHasBody( context ) )
                {
                    matchedBody = true;
                    continue;
                }

                return false;
            }

            return true;
        }

        bool ParameterHasRegisteredModelBinder( ParameterDescriptor parameter )
        {
            var modelMetadata = modelMetadataProvider.GetMetadataForType( parameter.ParameterType );
            var binderContext = new ModelBinderFactoryContext()
            {
                Metadata = modelMetadata,
                BindingInfo = parameter.BindingInfo,
                CacheToken = modelMetadata,
            };
            IModelBinder? binder;

            try
            {
                binder = modelBinderFactory.CreateBinder( binderContext );
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch
#pragma warning restore CA1031
            {
                binder = default;
            }

            return !( binder is SimpleTypeModelBinder ) &&
                   !( binder is BodyModelBinder ) &&
                   !( binder is ComplexTypeModelBinder ) &&
                   !( binder is BinderTypeModelBinder ) &&
                   !( binder is ICollectionModelBinder);
        }

        [DebuggerDisplay( "{Action.DisplayName,nq}" )]
        sealed class ActionIdAndParameters
        {
            internal ActionIdAndParameters( ActionDescriptor action, Func<ParameterDescriptor, bool> modelBound )
            {
                Action = action;

                var filteredParameters = new List<ParameterDescriptor>();

                foreach ( var parameter in action.Parameters )
                {
                    TotalParameterCount += 1;

                    if ( !parameter.ParameterType.IsModelBound() && !modelBound( parameter ) )
                    {
                        filteredParameters.Add( parameter );
                    }
                }

                FilteredParameters = filteredParameters.ToArray();
            }

            public int TotalParameterCount { get; }

            public IReadOnlyList<ParameterDescriptor> FilteredParameters { get; }

            public ActionDescriptor Action { get; }
        }
    }
}