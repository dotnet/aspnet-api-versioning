#if WEBAPI
namespace Microsoft.Web.Http.Versioning
#else
namespace Microsoft.AspNetCore.Mvc.Versioning
#endif
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public abstract class SelectVersionData : IEnumerable<object[]>
    {
        public abstract IEnumerator<object[]> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        protected static IEnumerable<ApiVersion> Supported( params ApiVersion[] versions ) => versions.AsEnumerable();

        protected static IEnumerable<ApiVersion> Deprecated( params ApiVersion[] versions ) => versions.AsEnumerable();

        protected static ApiVersion Expected( ApiVersion version ) => version;
    }
}