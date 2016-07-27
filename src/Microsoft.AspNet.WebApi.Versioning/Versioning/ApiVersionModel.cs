namespace Microsoft.Web.Http.Versioning
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using Versioning;

    /// <content>
    /// Provides the implementation for ASP.NET Web API.
    /// </content>
    public sealed partial class ApiVersionModel
    {
        private ApiVersionModel()
        {
            declaredVersions = defaultVersions;
            implementedVersions = defaultVersions;
            supportedVersions = defaultVersions;
            deprecatedVersions = emptyVersions;
        }

        internal ApiVersionModel( HttpControllerDescriptor controllerDescriptor )
        {
            Contract.Requires( controllerDescriptor != null );

            if ( IsApiVersionNeutral = controllerDescriptor.GetCustomAttributes<IApiVersionNeutral>( false ).Any() )
            {
                declaredVersions = emptyVersions;
                implementedVersions = emptyVersions;
                supportedVersions = emptyVersions;
                deprecatedVersions = emptyVersions;
            }
            else
            {
                declaredVersions = new Lazy<IReadOnlyList<ApiVersion>>( () => GetDeclaredControllerApiVersions( controllerDescriptor ) );
                implementedVersions = declaredVersions;
                supportedVersions = new Lazy<IReadOnlyList<ApiVersion>>( () => GetSupportedControllerApiVersions( controllerDescriptor ) );
                deprecatedVersions = new Lazy<IReadOnlyList<ApiVersion>>( controllerDescriptor.GetCustomAttributes<IApiVersionProvider>( false ).GetDeprecatedApiVersions );
            }
        }

        internal ApiVersionModel( HttpControllerDescriptor controllerDescriptor, ApiVersionModel aggregatedVersions )
        {
            Contract.Requires( controllerDescriptor != null );
            Contract.Requires( aggregatedVersions != null );

            if ( IsApiVersionNeutral = controllerDescriptor.GetCustomAttributes<IApiVersionNeutral>( false ).Any() )
            {
                declaredVersions = emptyVersions;
                implementedVersions = emptyVersions;
                supportedVersions = emptyVersions;
                deprecatedVersions = emptyVersions;
            }
            else
            {
                declaredVersions = new Lazy<IReadOnlyList<ApiVersion>>( () => GetDeclaredControllerApiVersions( controllerDescriptor ) );
                implementedVersions = aggregatedVersions.implementedVersions;
                supportedVersions = aggregatedVersions.supportedVersions;
                deprecatedVersions = aggregatedVersions.deprecatedVersions;
            }
        }

        internal ApiVersionModel( HttpActionDescriptor actionDescriptor )
        {
            Contract.Requires( actionDescriptor != null );

            if ( IsApiVersionNeutral = actionDescriptor.ControllerDescriptor.GetCustomAttributes<IApiVersionNeutral>( false ).Any() )
            {
                declaredVersions = emptyVersions;
                implementedVersions = emptyVersions;
                supportedVersions = emptyVersions;
                deprecatedVersions = emptyVersions;
            }
            else
            {
                declaredVersions = new Lazy<IReadOnlyList<ApiVersion>>( actionDescriptor.GetCustomAttributes<IApiVersionProvider>( false ).GetImplementedApiVersions );
                implementedVersions = declaredVersions;
                supportedVersions = new Lazy<IReadOnlyList<ApiVersion>>( actionDescriptor.GetCustomAttributes<IApiVersionProvider>( false ).GetSupportedApiVersions );
                deprecatedVersions = new Lazy<IReadOnlyList<ApiVersion>>( actionDescriptor.GetCustomAttributes<IApiVersionProvider>( false ).GetDeprecatedApiVersions );
            }
        }

        private static IReadOnlyList<ApiVersion> GetDeclaredControllerApiVersions( HttpControllerDescriptor controllerDescriptor )
        {
            Contract.Requires( controllerDescriptor != null );
            Contract.Ensures( Contract.Result<IReadOnlyList<ApiVersion>>() != null );

            var versions = controllerDescriptor.GetCustomAttributes<IApiVersionProvider>( false ).GetImplementedApiVersions();

            if ( versions.Count == 0 )
            {
                versions = new[] { controllerDescriptor.Configuration.GetDefaultApiVersion() };
            }

            return versions;
        }

        private static IReadOnlyList<ApiVersion> GetSupportedControllerApiVersions( HttpControllerDescriptor controllerDescriptor )
        {
            Contract.Requires( controllerDescriptor != null );
            Contract.Ensures( Contract.Result<IReadOnlyList<ApiVersion>>() != null );

            var versions = controllerDescriptor.GetCustomAttributes<IApiVersionProvider>( false ).GetSupportedApiVersions();

            if ( versions.Count == 0 )
            {
                versions = controllerDescriptor.GetCustomAttributes<IApiVersionProvider>( false ).GetImplementedApiVersions();

                if ( versions.Count == 0 )
                {
                    versions = new[] { controllerDescriptor.Configuration.GetDefaultApiVersion() };
                }
                else
                {
                    versions = emptyVersions.Value;
                }
            }

            return versions;
        }
    }
}