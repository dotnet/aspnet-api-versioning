#if WEBAPI
namespace Microsoft.Web.Http
#else
namespace Microsoft.AspNetCore.Mvc
#endif
{
#if !WEBAPI
    using Filters;
#endif
    using System;
#if WEBAPI
    using System.Web.Http.Filters;
#endif
    using static System.AttributeTargets;

    /// <summary>
    /// Represents an <see cref="ActionFilterAttribute">action filter</see> which reports API version information for
    /// an entire service or specific service action.
    /// </summary>
    [AttributeUsage( Class | Method, Inherited = true, AllowMultiple = false )]
    public sealed partial class ReportApiVersionsAttribute : ActionFilterAttribute
    {
        private const string ApiSupportedVersions = "api-supported-versions";
        private const string ApiDeprecatedVersions = "api-deprecated-versions";
    }
}
