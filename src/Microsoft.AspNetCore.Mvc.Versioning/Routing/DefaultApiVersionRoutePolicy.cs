namespace Microsoft.AspNetCore.Mvc.Routing
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Text;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Extensions;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.Internal;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using static ApiVersion;
    using static System.Environment;
    using static System.Linq.Enumerable;
    using static System.String;
    using static Versioning.ErrorCodes;

    /// <summary>
    /// Represents the default API versioning route policy.
    /// </summary>
    [CLSCompliant( false )]
    public class DefaultApiVersionRoutePolicy : IApiVersionRoutePolicy
    {
        readonly IOptions<ApiVersioningOptions> options;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultApiVersionRoutePolicy"/> class.
        /// </summary>
        /// <param name="errorResponseProvider">The <see cref="IErrorResponseProvider">provider</see> used to create error responses.</param>
        /// <param name="reportApiVersions">The <see cref="IReportApiVersions">object</see> used to report API versions.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        /// <param name="options">The <see cref="ApiVersioningOptions">options</see> associated with the route policy.</param>
        public DefaultApiVersionRoutePolicy(
            IErrorResponseProvider errorResponseProvider,
            IReportApiVersions reportApiVersions,
            ILoggerFactory loggerFactory,
            IOptions<ApiVersioningOptions> options )
        {
            Arg.NotNull( errorResponseProvider, nameof( errorResponseProvider ) );
            Arg.NotNull( reportApiVersions, nameof( reportApiVersions ) );
            Arg.NotNull( loggerFactory, nameof( loggerFactory ) );
            Arg.NotNull( options, nameof( options ) );

            ErrorResponseProvider = errorResponseProvider;
            ApiVersionReporter = reportApiVersions;
            Logger = loggerFactory.CreateLogger( GetType() );
            this.options = options;
        }

        /// <summary>
        /// Gets the error response provider associated with the route policy.
        /// </summary>
        /// <value>The <see cref="IErrorResponseProvider">provider</see> used to create error responses for the route policy.</value>
        protected IErrorResponseProvider ErrorResponseProvider { get; }

        /// <summary>
        /// Gets the object used to report API versions.
        /// </summary>
        /// <value>The associated <see cref="IReportApiVersions"/> object.</value>
        protected IReportApiVersions ApiVersionReporter { get; }

        /// <summary>
        /// Gets the logger associated with the route policy.
        /// </summary>
        /// <value>The associated <see cref="ILogger">logger</see>.</value>
        protected ILogger Logger { get; }

        /// <summary>
        /// Gets the configuration options associated with the route policy.
        /// </summary>
        /// <value>The associated <see cref="ApiVersioningOptions">service versioning options</see>.</value>
        protected ApiVersioningOptions Options => options.Value;

        /// <summary>
        /// Executes the API versioning route policy.
        /// </summary>
        /// <param name="context">The <see cref="RouteContext">route context</see> to evaluate against.</param>
        /// <param name="selectionResult">The <see cref="ActionSelectionResult">result</see> of action selection.</param>
        /// <returns>The <see cref="ActionDescriptor">action</see> conforming to the policy or <c>null</c>.</returns>
        public virtual ActionDescriptor Evaluate( RouteContext context, ActionSelectionResult selectionResult )
        {
            Arg.NotNull( context, nameof( context ) );
            Arg.NotNull( selectionResult, nameof( selectionResult ) );

            const ActionDescriptor NoMatch = default;

            switch ( selectionResult.MatchingActions.Count )
            {
                case 0:
                    OnUnmatched( context, selectionResult );
                    return NoMatch;
                case 1:
                    return OnSingleMatch( context, selectionResult );
            }

            OnMultipleMatches( context, selectionResult );
            return NoMatch;
        }

        /// <summary>
        /// Occurs when a single action is matched to the route policy.
        /// </summary>
        /// <param name="context">The current <see cref="RouteContext">route context</see>.</param>
        /// <param name="selectionResult">The current <see cref="ActionSelectionResult">action selection result</see>.</param>
        /// <returns>The single, matching <see cref="ActionDescriptor">action</see> conforming to the policy.</returns>
        protected virtual ActionDescriptor OnSingleMatch( RouteContext context, ActionSelectionResult selectionResult )
        {
            Arg.NotNull( context, nameof( context ) );
            Arg.NotNull( selectionResult, nameof( selectionResult ) );
            return selectionResult.BestMatch;
        }

        /// <summary>
        /// Occurs when a no actions are matched by the route policy.
        /// </summary>
        /// <param name="context">The current <see cref="RouteContext">route context</see>.</param>
        /// <param name="selectionResult">The current <see cref="ActionSelectionResult">action selection result</see>.</param>
        protected virtual void OnUnmatched( RouteContext context, ActionSelectionResult selectionResult )
        {
            Arg.NotNull( context, nameof( context ) );
            Arg.NotNull( selectionResult, nameof( selectionResult ) );

            context.Handler = ClientError( context.HttpContext, selectionResult );
        }

        /// <summary>
        /// Occurs when a multiple actions are matched to the route policy.
        /// </summary>
        /// <param name="context">The current <see cref="RouteContext">route context</see>.</param>
        /// <param name="selectionResult">The current <see cref="ActionSelectionResult">action selection result</see>.</param>
        /// <remarks>The default implementation always throws an <see cref="AmbiguousActionException"/>.</remarks>
        protected virtual void OnMultipleMatches( RouteContext context, ActionSelectionResult selectionResult )
        {
            Arg.NotNull( context, nameof( context ) );
            Arg.NotNull( selectionResult, nameof( selectionResult ) );

            var actionNames = Join( NewLine, selectionResult.MatchingActions.Select( ExpandActionSignature ) );

            Logger.AmbiguousActions( actionNames );

            var message = SR.ActionSelector_AmbiguousActions.FormatDefault( NewLine, actionNames );

            throw new AmbiguousActionException( message );
        }

        static string ExpandActionSignature( ActionDescriptor match )
        {
            Contract.Requires( match != null );
            Contract.Ensures( !IsNullOrEmpty( Contract.Result<string>() ) );

            if ( !( match is ControllerActionDescriptor action ) )
            {
                return match.DisplayName;
            }

            var signature = new StringBuilder();
            var controllerType = action.ControllerTypeInfo;

            signature.Append( controllerType.GetTypeDisplayName() );
            signature.Append( '.' );
            signature.Append( action.MethodInfo.Name );
            signature.Append( '(' );

            using ( var parameter = action.Parameters.GetEnumerator() )
            {
                if ( parameter.MoveNext() )
                {
                    var parameterType = parameter.Current.ParameterType;

                    signature.Append( parameterType.GetTypeDisplayName( false ) );

                    while ( parameter.MoveNext() )
                    {
                        parameterType = parameter.Current.ParameterType;
                        signature.Append( ", " );
                        signature.Append( parameterType.GetTypeDisplayName( false ) );
                    }
                }
            }

            signature.Append( ") (" );
            signature.Append( controllerType.Assembly.GetName().Name );
            signature.Append( ')' );

            return signature.ToString();
        }

        RequestHandler ClientError( HttpContext httpContext, ActionSelectionResult selectionResult )
        {
            Contract.Requires( httpContext != null );
            Contract.Requires( selectionResult != null );

            const RequestHandler NotFound = default;
            var candidates = selectionResult.CandidateActions;

            if ( candidates.Count == 0 )
            {
                return NotFound;
            }

            var feature = httpContext.Features.Get<IApiVersioningFeature>();
            var method = httpContext.Request.Method;
            var requestUrl = new Lazy<string>( httpContext.Request.GetDisplayUrl );
            var requestedVersion = feature.RawRequestedApiVersion;
            var parsedVersion = feature.RequestedApiVersion;
            var actionNames = new Lazy<string>( () => Join( NewLine, candidates.Select( a => a.DisplayName ) ) );
            var allowedMethods = new Lazy<HashSet<string>>( () => AllowedMethodsFromCandidates( candidates, parsedVersion ) );
            var apiVersions = new Lazy<ApiVersionModel>( candidates.Select( a => a.GetProperty<ApiVersionModel>() ).Aggregate );
            var handlerContext = new RequestHandlerContext( ErrorResponseProvider, ApiVersionReporter, apiVersions );
            var url = new Uri( requestUrl.Value );
            var safeUrl = url.SafeFullPath();

            if ( parsedVersion == null )
            {
                if ( IsNullOrEmpty( requestedVersion ) )
                {
                    if ( Options.AssumeDefaultVersionWhenUnspecified || candidates.Any( c => c.IsApiVersionNeutral() ) )
                    {
                        return VersionNeutralUnmatched( handlerContext, safeUrl, method, allowedMethods.Value, actionNames.Value );
                    }

                    return UnspecifiedApiVersion( handlerContext, actionNames.Value );
                }
                else if ( !TryParse( requestedVersion, out parsedVersion ) )
                {
                    return MalformedApiVersion( handlerContext, safeUrl, requestedVersion );
                }
            }
            else if ( IsNullOrEmpty( requestedVersion ) )
            {
                return VersionNeutralUnmatched( handlerContext, safeUrl, method, allowedMethods.Value, actionNames.Value );
            }
            else
            {
                requestedVersion = parsedVersion.ToString();
            }

            return Unmatched( handlerContext, safeUrl, method, allowedMethods.Value, actionNames.Value, parsedVersion, requestedVersion );
        }

        static HashSet<string> AllowedMethodsFromCandidates( IEnumerable<ActionDescriptor> candidates, ApiVersion apiVersion )
        {
            Contract.Requires( candidates != null );
            Contract.Ensures( Contract.Result<HashSet<string>>() != null );

            var httpMethods = new HashSet<string>( StringComparer.OrdinalIgnoreCase );

            foreach ( var candidate in candidates )
            {
                if ( candidate.ActionConstraints == null )
                {
                    continue;
                }
                else if ( apiVersion != null )
                {
                    if ( !candidate.IsMappedTo( apiVersion ) && !candidate.IsImplicitlyMappedTo( apiVersion ) )
                    {
                        continue;
                    }
                }

                foreach ( var constraint in candidate.ActionConstraints.OfType<HttpMethodActionConstraint>() )
                {
                    httpMethods.AddRange( constraint.HttpMethods );
                }
            }

            return httpMethods;
        }

        RequestHandler VersionNeutralUnmatched( RequestHandlerContext context, string requestUrl, string method, IReadOnlyCollection<string> allowedMethods, string actionNames )
        {
            Contract.Requires( context != null );
            Contract.Requires( !IsNullOrEmpty( requestUrl ) );
            Contract.Requires( !IsNullOrEmpty( method ) );
            Contract.Requires( allowedMethods != null );

            Logger.ApiVersionUnspecified( actionNames );
            context.Code = UnsupportedApiVersion;

            if ( allowedMethods.Count == 0 || allowedMethods.Contains( method ) )
            {
                context.Message = SR.VersionNeutralResourceNotSupported.FormatDefault( requestUrl );
                return new BadRequestHandler( context );
            }

            context.Message = SR.VersionNeutralMethodNotSupported.FormatDefault( requestUrl, method );
            context.AllowedMethods = allowedMethods.ToArray();

            return new MethodNotAllowedHandler( context );
        }

        RequestHandler UnspecifiedApiVersion( RequestHandlerContext context, string actionNames )
        {
            Contract.Requires( context != null );

            Logger.ApiVersionUnspecified( actionNames );

            context.Code = ApiVersionUnspecified;
            context.Message = SR.ApiVersionUnspecified;

            return new BadRequestHandler( context );
        }

        RequestHandler MalformedApiVersion( RequestHandlerContext context, string requestUrl, string requestedVersion )
        {
            Contract.Requires( context != null );
            Contract.Requires( !IsNullOrEmpty( requestUrl ) );
            Contract.Requires( !IsNullOrEmpty( requestedVersion ) );

            Logger.ApiVersionInvalid( requestedVersion );

            context.Code = InvalidApiVersion;
            context.Message = SR.VersionedResourceNotSupported.FormatDefault( requestUrl, requestedVersion );

            return new BadRequestHandler( context );
        }

        RequestHandler Unmatched(
            RequestHandlerContext context,
            string requestUrl,
            string method,
            IReadOnlyCollection<string> allowedMethods,
            string actionNames,
            ApiVersion parsedVersion,
            string requestedVersion )
        {
            Contract.Requires( context != null );
            Contract.Requires( !IsNullOrEmpty( requestUrl ) );
            Contract.Requires( !IsNullOrEmpty( method ) );
            Contract.Requires( allowedMethods != null );
            Contract.Requires( parsedVersion != null );
            Contract.Requires( !IsNullOrEmpty( requestedVersion ) );

            Logger.ApiVersionUnmatched( parsedVersion, actionNames );

            context.Code = UnsupportedApiVersion;

            if ( allowedMethods.Count == 0 || allowedMethods.Contains( method ) )
            {
                context.Message = SR.VersionedResourceNotSupported.FormatDefault( requestUrl, requestedVersion );
                return new BadRequestHandler( context );
            }

            context.Message = SR.VersionedMethodNotSupported.FormatDefault( requestUrl, requestedVersion, method );
            context.AllowedMethods = allowedMethods.ToArray();

            return new MethodNotAllowedHandler( context );
        }
    }
}