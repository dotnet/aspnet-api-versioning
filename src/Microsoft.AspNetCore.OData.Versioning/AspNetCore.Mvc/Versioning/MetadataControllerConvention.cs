namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ApplicationModels;
    using Microsoft.AspNetCore.Mvc.Versioning.Conventions;
    using System.Collections.Generic;
    using System.Reflection;

    // note: this convention needs to be applied after ApiVersionConvention.
    // there does not appear to be any guarantee or control over ensuring that happens.
    // the expected setup is:
    //
    // services.AddMvcCore();
    // services.AddApiVersioning();
    // services.AddOData().EnableApiVersioning();
    sealed class MetadataControllerConvention : IApplicationModelConvention
    {
        readonly TypeInfo metadataControllerType = typeof( MetadataController ).GetTypeInfo();
        readonly ODataApiVersioningOptions options;

        internal MetadataControllerConvention( ODataApiVersioningOptions options ) => this.options = options;

        public void Apply( ApplicationModel application )
        {
            var metadataController = default( ControllerModel );
            var supported = new HashSet<ApiVersion>();
            var deprecated = new HashSet<ApiVersion>();

            foreach ( var controller in application.Controllers )
            {
                if ( metadataControllerType.IsAssignableFrom( controller.ControllerType ) )
                {
                    metadataController = controller;
                }
                else
                {
                    var model = controller.GetProperty<ApiVersionModel>();

                    if ( model != null )
                    {
                        foreach ( var apiVersion in model.SupportedApiVersions )
                        {
                            supported.Add( apiVersion );
                        }

                        foreach ( var apiVersion in model.DeprecatedApiVersions )
                        {
                            deprecated.Add( apiVersion );
                        }
                    }
                }
            }

            if ( metadataController == null )
            {
                return;
            }

            deprecated.ExceptWith( supported );

            var conventions = options.Conventions;
            var builder = conventions.Controller( metadataController.ControllerType )
                                     .HasApiVersions( supported )
                                     .HasDeprecatedApiVersions( deprecated );

            builder.ApplyTo( metadataController );
        }
    }
}