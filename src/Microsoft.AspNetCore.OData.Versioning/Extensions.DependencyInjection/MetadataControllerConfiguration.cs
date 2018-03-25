namespace Microsoft.Extensions.DependencyInjection
{
    using AspNetCore.Mvc.Versioning;
    using Microsoft.AspNet.OData;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ApplicationModels;
    using Microsoft.AspNetCore.Mvc.Internal;
    using Microsoft.AspNetCore.Mvc.Versioning.Conventions;
    using Microsoft.Extensions.Options;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reflection;

    sealed class MetadataControllerConfiguration : IApplicationModelProvider
    {
        public MetadataControllerConfiguration( IOptions<MvcOptions> mvcOptions, IOptions<ApiVersioningOptions> apiVersioningOptions )
        {
            Options = mvcOptions;
            Conventions = apiVersioningOptions.Value.Conventions;
        }

        IOptions<MvcOptions> Options { get; }

        ApiVersionConventionBuilder Conventions { get; }

        public int Order => int.MaxValue;

        public void OnProvidersExecuted( ApplicationModelProviderContext context )
        {
            var supported = new HashSet<ApiVersion>();
            var deprecated = new HashSet<ApiVersion>();

            DiscoverODataApiVersions( context, supported, deprecated, out var controllerModel );

            if ( controllerModel == null )
            {
                return;
            }

            var builder = Conventions.Controller<VersionedMetadataController>()
                                     .HasApiVersions( supported )
                                     .HasDeprecatedApiVersions( deprecated );

            builder.ApplyTo( controllerModel );
        }

        public void OnProvidersExecuting( ApplicationModelProviderContext context ) { }

        void DiscoverODataApiVersions(
            ApplicationModelProviderContext context,
            HashSet<ApiVersion> supported,
            HashSet<ApiVersion> deprecated,
            out ControllerModel metadataControllerModel )
        {
            Contract.Requires( context != null );
            Contract.Requires( supported != null );
            Contract.Requires( deprecated != null );

            metadataControllerModel = null;

            var metadataController = typeof( VersionedMetadataController ).GetTypeInfo();

            foreach ( var controllerModel in GetControllerModels( context ) )
            {
                var controller = controllerModel.ControllerType;

                if ( metadataController.IsAssignableFrom( metadataController ) )
                {
                    metadataControllerModel = controllerModel;
                }

                var model = controllerModel.GetProperty<ApiVersionModel>();

                if ( model == null )
                {
                    continue;
                }

                foreach ( var apiVersion in model.SupportedApiVersions )
                {
                    supported.Add( apiVersion );
                }

                foreach ( var apiVersion in model.DeprecatedApiVersions )
                {
                    deprecated.Add( apiVersion );
                }
            }

            deprecated.ExceptWith( supported );
        }

        IEnumerable<ControllerModel> GetControllerModels( ApplicationModelProviderContext context )
        {
            Contract.Requires( context != null );
            Contract.Ensures( Contract.Result<IEnumerable<ControllerModel>>() != null );

            // TODO: is there a better way to do this? since we're already inside a pass of applying conventions, rebuild and apply
            // the conventions without modifying the current pass to avoid unwanted side effects.
            var provider = new DefaultApplicationModelProvider( Options );
            var modelContext = new ApplicationModelProviderContext( context.Result.Controllers.Select( c => c.ControllerType ) );

            provider.OnProvidersExecuting( modelContext );
            provider.OnProvidersExecuted( modelContext );

            ApplicationModelConventions.ApplyConventions( modelContext.Result, Options.Value.Conventions );

            return modelContext.Result.Controllers;
        }
    }
}