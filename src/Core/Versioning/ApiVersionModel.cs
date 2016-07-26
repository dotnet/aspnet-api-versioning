namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using ApplicationModels;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;

    /// <content>
    /// Provides the implementation for ASP.NET Core.
    /// </content>
    public sealed partial class ApiVersionModel
    {
        internal ApiVersionModel( ControllerModel controller )
        {
            Contract.Requires( controller != null );

            if ( IsApiVersionNeutral = controller.Attributes.OfType<ApiVersionNeutralAttribute>().Any() )
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

            if ( IsApiVersionNeutral = controller.Attributes.OfType<ApiVersionNeutralAttribute>().Any() )
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
    }
}
