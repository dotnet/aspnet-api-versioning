namespace Microsoft.Web.Http.Versioning.Conventions
{
    using System;
    using System.Web.Http.Controllers;

    /// <content>
    /// Provides additional implementation specific to ASP.NET Web API.
    /// </content>
    public partial interface IActionConventionBuilder<out T> where T : notnull, IHttpController
    {
    }
}