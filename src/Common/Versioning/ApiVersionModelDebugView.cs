#pragma warning disable CA1812

#if WEBAPI
namespace Microsoft.Web.Http.Versioning
#else
namespace Microsoft.AspNetCore.Mvc.Versioning
#endif
{
    using System;
    using static System.String;

    sealed class ApiVersionModelDebugView
    {
        const string Comma = ", ";
        readonly ApiVersionModel model;

        public ApiVersionModelDebugView( ApiVersionModel model ) => this.model = model;

        public bool VersionNeutral => model.IsApiVersionNeutral;

        public string Declared => Join( Comma, model.DeclaredApiVersions );

        public string Implemented => Join( Comma, model.ImplementedApiVersions );

        public string Supported => Join( Comma, model.SupportedApiVersions );

        public string Deprecated => Join( Comma, model.DeprecatedApiVersions );
    }
}