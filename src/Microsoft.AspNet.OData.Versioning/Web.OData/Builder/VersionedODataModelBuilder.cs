namespace Microsoft.Web.OData.Builder
{
    using Controllers;
    using Http;
    using Http.Versioning.Conventions;
    using Microsoft.OData.Edm;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Dispatcher;
    using System.Web.OData.Builder;

    /// <summary>
    /// Represents a versioned variant of the <see cref="ODataModelBuilder"/>.
    /// </summary>
    public class VersionedODataModelBuilder
    {
        readonly HttpConfiguration configuration;
        Func<ODataModelBuilder> modelBuilderFactory = () => new ODataConventionModelBuilder();

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionedODataModelBuilder"/>
        /// </summary>
        /// <param name="configuration">The <see cref="HttpConfiguration">HTTP configuration</see> associated with the builder.</param>
        /// <remarks>This constructor resolves the current <see cref="IHttpControllerSelector"/> from the
        /// <see cref="ServicesExtensions.GetHttpControllerSelector(ServicesContainer)"/> extension method via the
        /// <see cref="HttpConfiguration.Services"/>.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated by a code contract." )]
        public VersionedODataModelBuilder( HttpConfiguration configuration )
        {
            Arg.NotNull( configuration, nameof( configuration ) );
            this.configuration = configuration;
        }

        /// <summary>
        /// Gets or sets the factory method used to create model builders.
        /// </summary>
        /// <value>The factory <see cref="Func{TResult}">method</see> used to create <see cref="ODataModelBuilder">model builders</see>.</value>
        /// <remarks>The default implementation creates default instances of the <see cref="ODataConventionModelBuilder"/> class.</remarks>
        public Func<ODataModelBuilder> ModelBuilderFactory
        {
            get
            {
                Contract.Ensures( modelBuilderFactory != null );
                return modelBuilderFactory;
            }
            set
            {
                Arg.NotNull( value, nameof( value ) );
                modelBuilderFactory = value;
            }
        }

        /// <summary>
        /// Gets or sets the default model configuration.
        /// </summary>
        /// <value>The <see cref="Action{T1, T2}">method</see> for the default model configuration.
        /// The default value is <c>null</c>.</value>
        public Action<ODataModelBuilder, ApiVersion> DefaultModelConfiguration { get; set; }

        /// <summary>
        /// Gets the list of model configurations associated with the builder.
        /// </summary>
        /// <value>A <see cref="IList{T}">list</see> of model configurations associated with the builder.</value>
        public IList<IModelConfiguration> ModelConfigurations { get; } = new List<IModelConfiguration>();

        /// <summary>
        /// Gets or sets the action that is invoked after the <see cref="IEdmModel">EDM model</see> has been created.
        /// </summary>
        /// <value>The <see cref="Action{T1,T2}">action</see> to run after the model has been created. The default
        /// value is <c>null</c>.</value>
        public Action<ODataModelBuilder, IEdmModel> OnModelCreated { get; set; }

        IEnumerable<IModelConfiguration> GetMergedConfigurations()
        {
            Contract.Ensures( Contract.Result<IEnumerable<IModelConfiguration>>() != null );

            var configurations = ModelConfigurations.ToList();
            var defaultConfiguration = DefaultModelConfiguration;

            if ( defaultConfiguration != null )
            {
                configurations.Insert( 0, new DelegatingModelConfiguration( defaultConfiguration ) );
            }

            return configurations;
        }

        /// <summary>
        /// Builds and returns the sequence of EDM models based on the define model configurations.
        /// </summary>
        /// <returns>A <see cref="IEnumerable{T}">sequence</see> of <see cref="IEdmModel">EDM models</see>.</returns>
        [SuppressMessage( "Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Matches plural form of ODataModelBuilder.GetEdmModel(). A property would also not be appropriate." )]
        public virtual IEnumerable<IEdmModel> GetEdmModels()
        {
            var configurations = GetMergedConfigurations();
            var models = new List<IEdmModel>();
            var services = configuration.Services;
            var assembliesResolver = services.GetAssembliesResolver();
            var typeResolver = services.GetHttpControllerTypeResolver();
            var controllerTypes = typeResolver.GetControllerTypes( assembliesResolver ).Where( c => c.IsODataController() );
            var options = configuration.GetApiVersioningOptions();
            var supported = new HashSet<ApiVersion>();
            var deprecated = new HashSet<ApiVersion>();

            foreach ( var controllerType in controllerTypes )
            {
                var descriptor = new HttpControllerDescriptor( configuration, string.Empty, controllerType );

                options.Conventions.ApplyTo( descriptor );

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

            foreach ( var apiVersion in supported.Union( deprecated ) )
            {
                var builder = ModelBuilderFactory();

                foreach ( var configuration in configurations )
                {
                    configuration.Apply( builder, apiVersion );
                }

                var model = builder.GetEdmModel();

                model.SetAnnotationValue( model, new ApiVersionAnnotation( apiVersion ) );
                OnModelCreated?.Invoke( builder, model );
                models.Add( model );
            }

            ConfigureMetadataController( configuration, supported, deprecated );

            return models;
        }

        /// <summary>
        /// Configures the metadata controller using the specified configuration and API versions.
        /// </summary>
        /// <param name="configuration">The current <see cref="HttpConfiguration">configuration</see>.</param>
        /// <param name="supportedApiVersions">The discovered <see cref="IEnumerable{T}">sequence</see> of
        /// supported OData controller <see cref="ApiVersion">API versions</see>.</param>
        /// <param name="deprecatedApiVersions">The discovered <see cref="IEnumerable{T}">sequence</see> of
        /// deprecated OData controller <see cref="ApiVersion">API versions</see>.</param>
        protected virtual void ConfigureMetadataController( HttpConfiguration configuration, IEnumerable<ApiVersion> supportedApiVersions, IEnumerable<ApiVersion> deprecatedApiVersions )
        {
            var controllerMapping = configuration.Services.GetHttpControllerSelector().GetControllerMapping();

            if ( !controllerMapping.TryGetValue( "VersionedMetadata", out var controllerDescriptor ) )
            {
                return;
            }

            var options = configuration.GetApiVersioningOptions();
            var controllerBuilder = options.Conventions.Controller<VersionedMetadataController>()
                                                       .HasApiVersions( supportedApiVersions )
                                                       .HasDeprecatedApiVersions( deprecatedApiVersions );

            controllerBuilder.ApplyTo( controllerDescriptor );
        }
    }
}