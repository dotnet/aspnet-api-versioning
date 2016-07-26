#if WEBAPI
namespace System.Web.Http
#else
namespace Microsoft.AspNetCore.Mvc.Versioning
#endif
{
#if WEBAPI
    using Microsoft.Web.Http;
    using Microsoft.Web.Http.Versioning;
#endif
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;

    internal static class AttributeExtensions
    {
        internal static IReadOnlyList<ApiVersion> GetImplementedApiVersions<T>( this IEnumerable<T> attributes ) where T : IApiVersionProvider
        {
            Contract.Requires( attributes != null );
            Contract.Ensures( Contract.Result<IReadOnlyList<ApiVersion>>() != null );

            var versions = from attribute in attributes
                           where !attribute.AdvertiseOnly
                           from version in attribute.Versions
                           orderby version
                           select version;

            return versions.Distinct().ToArray();
        }

        internal static IReadOnlyList<ApiVersion> GetSupportedApiVersions<T>( this IEnumerable<T> attributes ) where T : IApiVersionProvider
        {
            Contract.Requires( attributes != null );
            Contract.Ensures( Contract.Result<IReadOnlyList<ApiVersion>>() != null );

            var versions = from attribute in attributes
                           where !attribute.Deprecated
                           from version in attribute.Versions
                           orderby version
                           select version;

            return versions.Distinct().ToArray();
        }

        internal static IReadOnlyList<ApiVersion> GetDeprecatedApiVersions<T>( this IEnumerable<T> attributes ) where T : IApiVersionProvider
        {
            Contract.Requires( attributes != null );
            Contract.Ensures( Contract.Result<IReadOnlyList<ApiVersion>>() != null );

            var versions = from attribute in attributes
                           where attribute.Deprecated
                           from version in attribute.Versions
                           orderby version
                           select version;

            return versions.Distinct().ToArray();
        }
    }
}
