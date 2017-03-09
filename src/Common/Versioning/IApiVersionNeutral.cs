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
    [SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces", Justification="While a marker interface, this interface is realized as attributes.")]
    public interface IApiVersionNeutral
    {
    }
}