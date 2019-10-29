namespace Microsoft.AspNet.OData.Builder
{
    using System;
    using System.Web.Http.Controllers;

    /// <content>
    /// Provides additional implementation specific to Microsoft ASP.NET Web API.
    /// </content>
    /// <typeparam name="T">The <see cref="Type">type</see> of <see cref="IHttpController">controller</see>.</typeparam>
    public partial class ODataActionQueryOptionsConventionBuilderCollection<T> where T : notnull, IHttpController
    {
    }
}