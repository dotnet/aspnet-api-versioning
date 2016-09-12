namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using ApplicationModels;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;

    /// <content>
    /// Provides the implementation for Microsoft ASP.NET Core.
    /// </content>
    public sealed partial class ApiVersionModel
    {
        internal ApiVersionModel( ControllerModel controller )
        {
            Contract.Requires( controller != null );

            if ( IsApiVersionNeutral = controller.Attributes.OfType<IApiVersionNeutral>().Any() )
            {
                declaredVersions = emptyVersions;
                implementedVersions = emptyVersions;
                supportedVersions = emptyVersions;
                deprecatedVersions = emptyVersions;
            }
            else
            {
                declaredVersions = new Lazy<IReadOnlyList<ApiVersion>>( controller.Attributes.OfType<IApiVersionProvider>().GetImplementedApiVersions );
                implementedVersions = declaredVersions;
                supportedVersions = new Lazy<IReadOnlyList<ApiVersion>>( controller.Attributes.OfType<IApiVersionProvider>().GetSupportedApiVersions );
                deprecatedVersions = new Lazy<IReadOnlyList<ApiVersion>>( controller.Attributes.OfType<IApiVersionProvider>().GetDeprecatedApiVersions );
            }
        }

        internal ApiVersionModel( ControllerModel controller, ActionModel action )
        {
            Contract.Requires( controller != null );

            if ( IsApiVersionNeutral = controller.Attributes.OfType<IApiVersionNeutral>().Any() )
            {
                declaredVersions = emptyVersions;
                implementedVersions = emptyVersions;
                supportedVersions = emptyVersions;
                deprecatedVersions = emptyVersions;
            }
            else
            {
                declaredVersions = new Lazy<IReadOnlyList<ApiVersion>>( action.Attributes.OfType<IApiVersionProvider>().GetImplementedApiVersions );
                implementedVersions = new Lazy<IReadOnlyList<ApiVersion>>( controller.Attributes.OfType<IApiVersionProvider>().GetImplementedApiVersions );
                supportedVersions = new Lazy<IReadOnlyList<ApiVersion>>( controller.Attributes.OfType<IApiVersionProvider>().GetSupportedApiVersions );
                deprecatedVersions = new Lazy<IReadOnlyList<ApiVersion>>( controller.Attributes.OfType<IApiVersionProvider>().GetDeprecatedApiVersions );
            }
        }

        internal ApiVersionModel(
            IEnumerable<ApiVersion> declared,
            IEnumerable<ApiVersion> supported,
            IEnumerable<ApiVersion> deprecated,
            IEnumerable<ApiVersion> advertised,
            IEnumerable<ApiVersion> deprecatedAdvertised )
        {
            Contract.Requires( declared != null );
            Contract.Requires( supported != null );
            Contract.Requires( deprecated != null );
            Contract.Requires( advertised != null );
            Contract.Requires( deprecatedAdvertised != null );

            declaredVersions = new Lazy<IReadOnlyList<ApiVersion>>( () => declared.OrderBy( v => v ).ToArray() );
            supportedVersions = new Lazy<IReadOnlyList<ApiVersion>>( () => supported.Union( advertised ).OrderBy( v => v ).ToArray() );
            deprecatedVersions = new Lazy<IReadOnlyList<ApiVersion>>( () => deprecated.Union( deprecatedAdvertised ).OrderBy( v => v ).ToArray() );
            implementedVersions = new Lazy<IReadOnlyList<ApiVersion>>( () => supportedVersions.Value.Union( deprecatedVersions.Value ).OrderBy( v => v ).ToArray() );
        }
    }
}