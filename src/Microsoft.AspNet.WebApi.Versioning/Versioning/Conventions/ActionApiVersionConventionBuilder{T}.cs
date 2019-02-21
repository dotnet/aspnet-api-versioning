namespace Microsoft.Web.Http.Versioning.Conventions
{
    using System;
    using System.Web.Http.Controllers;

    /// <content>
    /// Provides additional implementation specific to Microsoft ASP.NET Web API.
    /// </content>
    public partial class ActionApiVersionConventionBuilder<T> : IActionConventionBuilder<T> where T : IHttpController
    {
    }
}