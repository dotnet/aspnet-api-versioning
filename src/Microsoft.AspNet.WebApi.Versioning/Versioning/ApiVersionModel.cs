namespace Microsoft.Web.Http.Versioning
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Web.Http;
    using System.Web.Http.Controllers;

    /// <content>
    /// Provides the implementation for ASP.NET Web API.
    /// </content>
    public sealed partial class ApiVersionModel
    {
        ApiVersionModel()
        {
            declaredVersions = defaultVersions;
            implementedVersions = defaultVersions;
            supportedVersions = defaultVersions;
            deprecatedVersions = emptyVersions;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersionModel"/> class.
        /// </summary>
        /// <param name="controllerDescriptor">The <see cref="HttpControllerDescriptor"/> to initialize the API version model from.</param>
        public ApiVersionModel( HttpControllerDescriptor controllerDescriptor )
        {
            Arg.NotNull( controllerDescriptor, nameof( controllerDescriptor ) );

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

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersionModel"/> class.
        /// </summary>
        /// <param name="actionDescriptor">The <see cref="HttpActionDescriptor"/> to initialize the API version model from.</param>
        public ApiVersionModel( HttpActionDescriptor actionDescriptor )
        {
            Arg.NotNull( actionDescriptor, nameof( actionDescriptor ) );

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

        static IReadOnlyList<ApiVersion> GetDeclaredControllerApiVersions( HttpControllerDescriptor controllerDescriptor )
        {
            Contract.Requires( controllerDescriptor != null );
            Contract.Ensures( Contract.Result<IReadOnlyList<ApiVersion>>() != null );

            var versions = controllerDescriptor.GetCustomAttributes<IApiVersionProvider>( false ).GetImplementedApiVersions();

            if ( versions.Count == 0 )
            {
                versions = new[] { controllerDescriptor.Configuration.GetApiVersioningOptions().DefaultApiVersion };
            }

            return versions;
        }

        static IReadOnlyList<ApiVersion> GetSupportedControllerApiVersions( HttpControllerDescriptor controllerDescriptor )
        {
            Contract.Requires( controllerDescriptor != null );
            Contract.Ensures( Contract.Result<IReadOnlyList<ApiVersion>>() != null );

            var versions = controllerDescriptor.GetCustomAttributes<IApiVersionProvider>( false ).GetSupportedApiVersions();

            if ( versions.Count == 0 )
            {
                versions = controllerDescriptor.GetCustomAttributes<IApiVersionProvider>( false ).GetImplementedApiVersions();

                if ( versions.Count == 0 )
                {
                    versions = new[] { controllerDescriptor.Configuration.GetApiVersioningOptions().DefaultApiVersion };
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