#if WEBAPI
namespace Microsoft.Web.Http.Versioning
#else
namespace Microsoft.AspNetCore.Mvc.Versioning
#endif
{
    using System;
    using System.Diagnostics.Contracts;

    internal sealed class ApiVersionModelDebugView
    {
        private readonly ApiVersionModel model;

        public ApiVersionModelDebugView( ApiVersionModel model )
        {
            Contract.Requires( model != null );
            this.model = model;
        }

        public bool VersionNeutral => model.IsApiVersionNeutral;

        public string Declared => string.Join( ", ", model.DeclaredApiVersions );

        public string Implemented => string.Join( ", ", model.ImplementedApiVersions );

        public string Supported => string.Join( ", ", model.SupportedApiVersions );

        public string Deprecated => string.Join( ", ", model.DeprecatedApiVersions );
    }
}
