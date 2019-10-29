namespace Microsoft.AspNetCore.Mvc.Routing
{
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.AspNetCore.Routing;
    using System;

    /// <summary>
    /// Defines the behavior of an API version route policy.
    /// </summary>
    [CLSCompliant( false )]
    public interface IApiVersionRoutePolicy
    {
        /// <summary>
        /// Executes the API versioning route policy.
        /// </summary>
        /// <param name="context">The <see cref="RouteContext">route context</see> to evaluate against.</param>
        /// <param name="selectionResult">The <see cref="ActionSelectionResult">result</see> of action selection.</param>
        /// <returns>The <see cref="ActionDescriptor">action</see> conforming to the policy or <c>null</c>.</returns>
        ActionDescriptor? Evaluate( RouteContext context, ActionSelectionResult selectionResult );
    }
}