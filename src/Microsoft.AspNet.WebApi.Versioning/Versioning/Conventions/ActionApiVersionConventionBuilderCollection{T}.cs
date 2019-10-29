namespace Microsoft.Web.Http.Versioning.Conventions
{
    using System;
    using System.Web.Http.Controllers;

    /// <content>
    /// Provides additional implementation specific to Microsoft ASP.NET Web API.
    /// </content>
    /// <typeparam name="T">The <see cref="Type">type</see> of <see cref="IHttpController">controller</see>.</typeparam>
    public partial class ActionApiVersionConventionBuilderCollection<T> where T : notnull, IHttpController
    {
    }
}