namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Microsoft.AspNetCore.Mvc.Routing;
    using System;

    /// <summary>
    /// Defines the behavior of the API versioning feature.
    /// </summary>
    [CLSCompliant( false )]
    public interface IApiVersioningFeature
    {
        /// <summary>
        /// Gets or sets the name of the route parameter containing the API Version value.
        /// </summary>
        /// <value>The name of the API version route parameter or <c>null</c>.</value>
        /// <remarks>This property will be <c>null</c> unless versioning by URL segment and the incoming request
        /// matches the <see cref="ApiVersionRouteConstraint">API version route constraint</see>.</remarks>
        string? RouteParameter { get; set; }

        /// <summary>
        /// Gets or sets the raw, unparsed API version for the current request.
        /// </summary>
        /// <value>The unparsed API version value for the current request.</value>
        string? RawRequestedApiVersion { get; set; }

        /// <summary>
        /// Gets or sets the API version for the current request.
        /// </summary>
        /// <value>The current <see cref="ApiVersion">API version</see> for the current request.</value>
        /// <remarks>If an API version was not provided for the current request or the value
        /// provided is invalid, this property will return <c>null</c>.</remarks>
        ApiVersion? RequestedApiVersion { get; set; }

        /// <summary>
        /// Gets the action selection result associated with the current request.
        /// </summary>
        /// <value>The <see cref="ActionSelectionResult">action selection result</see> associated with the current request.</value>
        ActionSelectionResult SelectionResult { get; }
    }
}