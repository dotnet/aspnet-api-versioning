#if WEBAPI
namespace Microsoft.Web.Http
#else
namespace Microsoft.AspNetCore.Mvc
#endif
{
#if WEBAPI
    using Microsoft.Web.Http.Versioning;
#else
    using Microsoft.AspNetCore.Mvc.Versioning;
#endif
    using System;
    using static System.AttributeTargets;

    /// <summary>
    /// Represents the metadata to indicate a service is API version neutral.
    /// </summary>
    [AttributeUsage( Class | Method, AllowMultiple = false, Inherited = true )]
    public sealed class ApiVersionNeutralAttribute : Attribute, IApiVersionNeutral
    {
    }
}