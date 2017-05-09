namespace Microsoft.AspNetCore.Mvc.Routing
{
    using Microsoft.AspNetCore.Routing;
    using System;

    /// <summary>
    /// Defines the behavior of an API version route policy.
    /// </summary>
    [CLSCompliant( false )]
    public interface IApiVersionRoutePolicy : IRouter
    {
    }
}