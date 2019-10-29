#pragma warning disable CA1040

#if WEBAPI
namespace Microsoft.Web.Http.Versioning
#else
namespace Microsoft.AspNetCore.Mvc.Versioning
#endif
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Defines the behavior of a service that is API version-neutral.
    /// </summary>
    public interface IApiVersionNeutral
    {
    }
}