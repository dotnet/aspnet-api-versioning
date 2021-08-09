namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents the context for selecting a controller action from a list of matching candidates.
    /// </summary>
    [CLSCompliant( false )]
    public class ActionSelectionContext
    {
        readonly Lazy<ApiVersionModel> allVersions;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionSelectionContext"/> class.
        /// </summary>
        /// <param name="httpContext">The current <see cref="HttpContext">HTTP context</see>.</param>
        /// <param name="matchingActions">The <see cref="IReadOnlyList{T}">read-only list</see> of <see cref="ActionDescriptor">actions</see> matching the current route.</param>
        /// <param name="requestedVersion">The currently requested <see cref="ApiVersion"/>. This parameter can be <c>null</c>.</param>
        public ActionSelectionContext( HttpContext httpContext, IReadOnlyList<ActionDescriptor> matchingActions, ApiVersion? requestedVersion )
        {
            allVersions = new Lazy<ApiVersionModel>( CreateAggregatedModel );
            HttpContext = httpContext;
            MatchingActions = matchingActions;
            RequestedVersion = requestedVersion;
        }

        /// <summary>
        /// Gets the current HTTP context.
        /// </summary>
        /// <value>The current <see cref="HttpContext">HTTP context</see>.</value>
        public HttpContext HttpContext { get; }

        /// <summary>
        /// Gets the read-only list of controller actions matching the current route.
        /// </summary>
        /// <value>A <see cref="IReadOnlyList{T}">read-only list</see> of <see cref="ActionDescriptor">actions</see> matching the current route.</value>
        public IReadOnlyList<ActionDescriptor> MatchingActions { get; }

        /// <summary>
        /// Gets the model for the context that contains the aggregation of all service API versions.
        /// </summary>
        /// <value>An aggregated <see cref="ApiVersionModel">model</see> that contains the service API versions of all matching actions.</value>
        public ApiVersionModel AllVersions => allVersions.Value;

        /// <summary>
        /// Gets or sets the currently requested API version.
        /// </summary>
        /// <value>The currently requested <see cref="ApiVersion">API version</see>.</value>
        /// <remarks>This property may be <c>null</c> if the client did request an explicit version. Implementors should update this property when
        /// implicit API version matching is allowed and a version has been selected.</remarks>
        public ApiVersion? RequestedVersion { get; set; }

        ApiVersionModel CreateAggregatedModel() => MatchingActions.Select( action => action.GetApiVersionModel() ).Aggregate();
    }
}