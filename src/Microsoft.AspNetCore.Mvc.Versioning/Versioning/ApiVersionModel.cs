namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using ApplicationModels;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <content>
    /// Provides the implementation for Microsoft ASP.NET Core.
    /// </content>
    public sealed partial class ApiVersionModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersionModel"/> class.
        /// </summary>
        /// <param name="controllerModel">The <see cref="ControllerModel"/> to initialize the API version model from.</param>
        [CLSCompliant( false )]
        public ApiVersionModel( ControllerModel controllerModel )
        {
            Arg.NotNull( controllerModel, nameof( controllerModel ) );

            if ( IsApiVersionNeutral = controllerModel.Attributes.OfType<IApiVersionNeutral>().Any() )
            {
                declaredVersions = emptyVersions;
                implementedVersions = emptyVersions;
                supportedVersions = emptyVersions;
                deprecatedVersions = emptyVersions;
            }
            else
            {
                declaredVersions = new Lazy<IReadOnlyList<ApiVersion>>( controllerModel.Attributes.OfType<IApiVersionProvider>().GetImplementedApiVersions );
                implementedVersions = declaredVersions;
                supportedVersions = new Lazy<IReadOnlyList<ApiVersion>>( controllerModel.Attributes.OfType<IApiVersionProvider>().GetSupportedApiVersions );
                deprecatedVersions = new Lazy<IReadOnlyList<ApiVersion>>( controllerModel.Attributes.OfType<IApiVersionProvider>().GetDeprecatedApiVersions );
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersionModel"/> class.
        /// </summary>
        /// <param name="controllerModel">The <see cref="ControllerModel"/> to initialize the API version model from.</param>
        /// <param name="actionModel">The <see cref="ActionModel"/> to initialize the API version model from.</param>
        [CLSCompliant( false )]
        public ApiVersionModel( ControllerModel controllerModel, ActionModel actionModel )
        {
            Arg.NotNull( controllerModel, nameof( controllerModel ) );
            Arg.NotNull( actionModel, nameof( actionModel ) );

            var versionModel = controllerModel.GetProperty<ApiVersionModel>();

            if ( versionModel == null )
            {
                if ( IsApiVersionNeutral = controllerModel.Attributes.OfType<IApiVersionNeutral>().Any() )
                {
                    declaredVersions = emptyVersions;
                    implementedVersions = emptyVersions;
                    supportedVersions = emptyVersions;
                    deprecatedVersions = emptyVersions;
                }
                else
                {
                    declaredVersions = new Lazy<IReadOnlyList<ApiVersion>>( actionModel.Attributes.OfType<IApiVersionProvider>().GetImplementedApiVersions );
                    implementedVersions = new Lazy<IReadOnlyList<ApiVersion>>( controllerModel.Attributes.OfType<IApiVersionProvider>().GetImplementedApiVersions );
                    supportedVersions = new Lazy<IReadOnlyList<ApiVersion>>( controllerModel.Attributes.OfType<IApiVersionProvider>().GetSupportedApiVersions );
                    deprecatedVersions = new Lazy<IReadOnlyList<ApiVersion>>( controllerModel.Attributes.OfType<IApiVersionProvider>().GetDeprecatedApiVersions );
                }
            }
            else
            {
                if ( IsApiVersionNeutral = versionModel.IsApiVersionNeutral )
                {
                    declaredVersions = emptyVersions;
                    implementedVersions = emptyVersions;
                    supportedVersions = emptyVersions;
                    deprecatedVersions = emptyVersions;
                }
                else
                {
                    declaredVersions = new Lazy<IReadOnlyList<ApiVersion>>( actionModel.Attributes.OfType<IApiVersionProvider>().GetImplementedApiVersions );
                    implementedVersions = new Lazy<IReadOnlyList<ApiVersion>>( () => versionModel.ImplementedApiVersions );
                    supportedVersions = new Lazy<IReadOnlyList<ApiVersion>>( () => versionModel.SupportedApiVersions );
                    deprecatedVersions = new Lazy<IReadOnlyList<ApiVersion>>( () => versionModel.DeprecatedApiVersions );
                }
            }
        }
    }
}