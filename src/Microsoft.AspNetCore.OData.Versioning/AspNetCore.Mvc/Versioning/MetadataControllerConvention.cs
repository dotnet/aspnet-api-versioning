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
        readonly ODataApiVersioningOptions options;

        internal MetadataControllerConvention( ODataApiVersioningOptions options ) => this.options = options;

        public void Apply( ApplicationModel application )
        {
            var metadataControllers = new List<ControllerModel>();
            var supported = new HashSet<ApiVersion>();
            var deprecated = new HashSet<ApiVersion>();

            for ( var i = 0; i < application.Controllers.Count; i++ )
            {
                var controller = application.Controllers[i];

                if ( controller.ControllerType.IsMetadataController() )
                {
                    metadataControllers.Add( controller );
                    continue;
                }

                for ( var j = 0; j < controller.Actions.Count; j++ )
                {
                    var action = controller.Actions[j];
                    var model = action.GetProperty<ApiVersionModel>();

                    if ( model == null )
                    {
                        continue;
                    }

                    for ( var k = 0; k < model.SupportedApiVersions.Count; k++ )
                    {
                        supported.Add( model.SupportedApiVersions[k] );
                    }

                    for ( var k = 0; k < model.DeprecatedApiVersions.Count; k++ )
                    {
                        deprecated.Add( model.DeprecatedApiVersions[k] );
                    }
                }
            }

            var metadataController = SelectBestMetadataController( metadataControllers );

            if ( metadataController == null )
            {
                // graceful exit; in theory, this should never happen
                return;
            }

            deprecated.ExceptWith( supported );

            var conventions = options.Conventions;
            var builder = conventions.Controller( metadataController.ControllerType )
                                     .HasApiVersions( supported )
                                     .HasDeprecatedApiVersions( deprecated );

            builder.ApplyTo( metadataController );
        }

        static ControllerModel? SelectBestMetadataController( IReadOnlyList<ControllerModel> controllers )
        {
            // note: there should be at least 2 metadata controllers, but there could be 3+
            // if a developer defines their own custom controller. ultimately, there an be
            // only one. choose and version the best controller using the following ranking:
            //
            // 1. original MetadataController type
            // 2. VersionedMetadataController type (it's possible this has been removed upstream)
            // 3. last, custom type of MetadataController from another assembly
            var bestController = default( ControllerModel );
            var original = typeof( MetadataController ).GetTypeInfo();
            var versioned = typeof( VersionedMetadataController ).GetTypeInfo();

            for ( var i = 0; i < controllers.Count; i++ )
            {
                var controller = controllers[i];

                if ( bestController == default )
                {
                    bestController = controller;
                }
                else if ( bestController.ControllerType == original &&
                          controller.ControllerType == versioned )
                {
                    bestController = controller;
                }
                else if ( bestController.ControllerType == versioned &&
                          controller.ControllerType != original )
                {
                    bestController = controller;
                }
                else if ( bestController.ControllerType != versioned &&
                          controller.ControllerType != original )
                {
                    bestController = controller;
                }
            }

            return bestController;
        }
    }
}