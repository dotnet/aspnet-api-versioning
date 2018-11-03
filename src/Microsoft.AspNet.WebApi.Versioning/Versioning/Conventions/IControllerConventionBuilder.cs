namespace Microsoft.Web.Http.Versioning.Conventions
{
    using System;
    using System.Web.Http.Controllers;

    /// <summary>
    /// Defines the behavior of a convention builder for a controller.
    /// </summary>
    public interface IControllerConventionBuilder : IApiVersionConventionBuilder, IApiVersionConvention<HttpControllerDescriptor>
    {
    }
}