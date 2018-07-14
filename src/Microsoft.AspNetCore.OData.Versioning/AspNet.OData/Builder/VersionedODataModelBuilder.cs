namespace Microsoft.AspNet.OData.Builder
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.Extensions.Options;
    using Microsoft.OData.Edm;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using static System.Linq.Enumerable;

    /// <content>
    /// Provides additional implementation specific to ASP.NET Core.
    /// </content>
    [CLSCompliant( false )]
    public partial class VersionedODataModelBuilder
    {
        readonly IOptions<ApiVersioningOptions> options;

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionedODataModelBuilder"/> class.
        /// </summary>
        /// <param name="actionDescriptorCollectionProvider">The <see cref="IActionDescriptorCollectionProvider "/> used to discover OData routes.</param>
        /// <param name="options">The <see cref="ApiVersioningOptions">options</see> associated with the action selector.</param>
        public VersionedODataModelBuilder( IActionDescriptorCollectionProvider actionDescriptorCollectionProvider, IOptions<ApiVersioningOptions> options )
            : this( actionDescriptorCollectionProvider, options, Empty<IModelConfiguration>() ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionedODataModelBuilder"/> class.
        /// </summary>
        /// <param name="actionDescriptorCollectionProvider">The <see cref="IActionDescriptorCollectionProvider "/> used to discover OData routes.</param>
        /// <param name="options">The <see cref="ApiVersioningOptions">options</see> associated with the action selector.</param>
        /// <param name="modelConfigurations">The <see cref="IEnumerable{T}">sequence</see> of
        /// <see cref="IModelConfiguration">model configurations</see> associated with the model builder.</param>
        public VersionedODataModelBuilder(
            IActionDescriptorCollectionProvider actionDescriptorCollectionProvider,
            IOptions<ApiVersioningOptions> options,
            IEnumerable<IModelConfiguration> modelConfigurations )
        {
            Arg.NotNull( actionDescriptorCollectionProvider, nameof( actionDescriptorCollectionProvider ) );
            Arg.NotNull( options, nameof( options ) );
            Arg.NotNull( modelConfigurations, nameof( modelConfigurations ) );

            ActionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
            this.options = options;

            foreach ( var configuration in modelConfigurations )
            {
                ModelConfigurations.Add( configuration );
            }
        }

        /// <summary>
        /// Gets the action descriptors used to discover OData routes.
        /// </summary>
        /// <value>The <see cref="IActionDescriptorCollectionProvider "/> used to discover OData routes.</value>
        protected IActionDescriptorCollectionProvider ActionDescriptorCollectionProvider { get; }

        /// <summary>
        /// Gets the configuration options associated with the builder.
        /// </summary>
        /// <value>The associated <see cref="ApiVersioningOptions">API versioning options</see>.</value>
        protected ApiVersioningOptions Options => options.Value;

        /// <summary>
        /// Builds and returns the sequence of EDM models based on the define model configurations.
        /// </summary>
        /// <returns>A <see cref="IEnumerable{T}">sequence</see> of <see cref="IEdmModel">EDM models</see>.</returns>
        public virtual IEnumerable<IEdmModel> GetEdmModels()
        {
            Contract.Ensures( Contract.Result<IEnumerable<IEdmModel>>() != null );

            var apiVersions = GetApiVersions();
            var configurations = GetMergedConfigurations();
            var models = new List<IEdmModel>();

            BuildModelPerApiVersion( apiVersions, configurations, models );

            return models;
        }

        /// <summary>
        /// Gets the API versions for all known OData routes.
        /// </summary>
        /// <returns>The <see cref="IEnumerable{T}">sequence</see> of <see cref="ApiVersion">API versions</see>
        /// for all known OData routes.</returns>
        protected virtual IEnumerable<ApiVersion> GetApiVersions()
        {
            Contract.Ensures( Contract.Result<IEnumerable<ApiVersion>>() != null );

            var actions = ActionDescriptorCollectionProvider.ActionDescriptors.Items.OfType<ControllerActionDescriptor>();
            var implemented = new HashSet<ApiVersion>();

            foreach ( var action in actions )
            {
                if ( !action.ControllerTypeInfo.IsODataController() )
                {
                    continue;
                }

                var model = action.GetProperty<ApiVersionModel>();

                if ( model == null )
                {
                    continue;
                }

                foreach ( var apiVersion in model.ImplementedApiVersions )
                {
                    implemented.Add( apiVersion );
                }
            }

            if ( implemented.Count == 0 )
            {
                implemented.Add( Options.DefaultApiVersion );
            }

            var apiVersions = implemented.ToArray();

            if ( apiVersions.Length > 1 )
            {
                Array.Sort( apiVersions );
            }

            return apiVersions;
        }
    }
}