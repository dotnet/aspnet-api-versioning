namespace Microsoft.AspNet.OData.Builder
{
    using Microsoft.OData.Edm;
    using Microsoft.Web.Http;
    using Microsoft.Web.Http.Versioning;
    using Microsoft.Web.Http.Versioning.Conventions;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Dispatcher;

    /// <content>
    /// Provides additional implementation specific to ASP.NET Web API.
    /// </content>
    public partial class VersionedODataModelBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VersionedODataModelBuilder"/> class.
        /// </summary>
        /// <param name="configuration">The <see cref="HttpConfiguration">HTTP configuration</see> associated with the builder.</param>
        /// <remarks>This constructor resolves the current <see cref="IHttpControllerSelector"/> from the
        /// <see cref="ServicesExtensions.GetHttpControllerSelector(ServicesContainer)"/> extension method via the
        /// <see cref="HttpConfiguration.Services"/>.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated by a code contract." )]
        public VersionedODataModelBuilder( HttpConfiguration configuration )
        {
            Arg.NotNull( configuration, nameof( configuration ) );
            Configuration = configuration;
        }

        /// <summary>
        /// Gets the associated HTTP configuration.
        /// </summary>
        /// <value>The <see cref="HttpConfiguration">HTTP configuration</see> associated with the builder.</value>
        protected HttpConfiguration Configuration { get; }

        /// <summary>
        /// Gets the API versioning options associated with the builder.
        /// </summary>
        /// <value>The configured <see cref="ApiVersioningOptions">API versioning options</see>.</value>
        protected ApiVersioningOptions Options => Configuration.GetApiVersioningOptions();

        /// <summary>
        /// Builds and returns the sequence of EDM models based on the define model configurations.
        /// </summary>
        /// <returns>A <see cref="IEnumerable{T}">sequence</see> of <see cref="IEdmModel">EDM models</see>.</returns>
        [SuppressMessage( "Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Matches plural form of ODataModelBuilder.GetEdmModel(). A property would also not be appropriate." )]
        public virtual IEnumerable<IEdmModel> GetEdmModels()
        {
            Contract.Ensures( Contract.Result<IEnumerable<IEdmModel>>() != null );

            var configurations = GetMergedConfigurations();
            var models = new List<IEdmModel>();
            var services = Configuration.Services;
            var assembliesResolver = services.GetAssembliesResolver();
            var typeResolver = services.GetHttpControllerTypeResolver();
            var controllerTypes = typeResolver.GetControllerTypes( assembliesResolver ).Where( c => c.IsODataController() );
            var conventions = Options.Conventions;
            var supported = new HashSet<ApiVersion>();
            var deprecated = new HashSet<ApiVersion>();

            foreach ( var controllerType in controllerTypes )
            {
                var descriptor = new HttpControllerDescriptor( Configuration, string.Empty, controllerType );

                conventions.ApplyTo( descriptor );

                var model = descriptor.GetApiVersionModel();

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
            BuildModelPerApiVersion( supported.Union( deprecated ), configurations, models );
            ConfigureMetadataController( supported, deprecated );

            return models;
        }

        /// <summary>
        /// Configures the metadata controller using the specified configuration and API versions.
        /// </summary>
        /// <param name="supportedApiVersions">The discovered <see cref="IEnumerable{T}">sequence</see> of
        /// supported OData controller <see cref="ApiVersion">API versions</see>.</param>
        /// <param name="deprecatedApiVersions">The discovered <see cref="IEnumerable{T}">sequence</see> of
        /// deprecated OData controller <see cref="ApiVersion">API versions</see>.</param>
        protected virtual void ConfigureMetadataController( IEnumerable<ApiVersion> supportedApiVersions, IEnumerable<ApiVersion> deprecatedApiVersions )
        {
            var controllerMapping = Configuration.Services.GetHttpControllerSelector().GetControllerMapping();

            if ( !controllerMapping.TryGetValue( "VersionedMetadata", out var controllerDescriptor ) )
            {
                return;
            }

            var conventions = Options.Conventions;
            var controllerBuilder = conventions.Controller<VersionedMetadataController>()
                                               .HasApiVersions( supportedApiVersions )
                                               .HasDeprecatedApiVersions( deprecatedApiVersions );

            controllerBuilder.ApplyTo( controllerDescriptor );
        }
    }
}