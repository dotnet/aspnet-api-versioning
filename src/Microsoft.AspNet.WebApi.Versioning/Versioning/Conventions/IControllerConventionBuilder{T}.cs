namespace Microsoft.Web.Http.Versioning.Conventions
{
    using System;
    using System.Web.Http.Controllers;

    /// <content>
    /// Provides additional implementation specific to ASP.NET Web API.
    /// </content>
    public partial interface IControllerConventionBuilder<out T> : IApiVersionConvention<HttpControllerDescriptor>
    {
    }
}