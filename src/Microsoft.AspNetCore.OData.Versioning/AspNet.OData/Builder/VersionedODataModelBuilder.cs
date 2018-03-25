namespace Microsoft.AspNet.OData.Builder
{
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.OData.Edm;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using static System.Linq.Enumerable;

    /// <content>
    /// Provides additional implementation specific to ASP.NET Web API.
    /// </content>
    [CLSCompliant( false )]
    public partial class VersionedODataModelBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VersionedODataModelBuilder"/> class.
        /// </summary>
        /// <param name="apiVersionProvider">The <see cref="IODataApiVersionProvider"/> associated with the model builder.</param>
        public VersionedODataModelBuilder( IODataApiVersionProvider apiVersionProvider ) : this( apiVersionProvider, Empty<IModelConfiguration>() ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionedODataModelBuilder"/> class.
        /// </summary>
        /// <param name="apiVersionProvider">The <see cref="IODataApiVersionProvider"/> associated with the model builder.</param>
        /// <param name="modelConfigurations">The <see cref="IEnumerable{T}">sequence</see> of
        /// <see cref="IModelConfiguration">model configurations</see> associated with the model builder.</param>
        public VersionedODataModelBuilder( IODataApiVersionProvider apiVersionProvider, IEnumerable<IModelConfiguration> modelConfigurations )
        {
            Arg.NotNull( apiVersionProvider, nameof( apiVersionProvider ) );
            Arg.NotNull( modelConfigurations, nameof( modelConfigurations ) );

            ApiVersionProvider = apiVersionProvider;

            foreach ( var configuration in modelConfigurations )
            {
                ModelConfigurations.Add( configuration );
            }
        }

        /// <summary>
        /// Gets the OData API version provider.
        /// </summary>
        /// <value>The <see cref="IODataApiVersionProvider"/> associated with the model builder.</value>
        protected IODataApiVersionProvider ApiVersionProvider { get; }

        /// <summary>
        /// Builds and returns the sequence of EDM models based on the define model configurations.
        /// </summary>
        /// <returns>A <see cref="IEnumerable{T}">sequence</see> of <see cref="IEdmModel">EDM models</see>.</returns>
        public virtual IEnumerable<IEdmModel> GetEdmModels()
        {
            Contract.Ensures( Contract.Result<IEnumerable<IEdmModel>>() != null );

            var apiVersions = ApiVersionProvider.SupportedApiVersions.Union( ApiVersionProvider.DeprecatedApiVersions );
            var configurations = GetMergedConfigurations();
            var models = new List<IEdmModel>();

            BuildModelPerApiVersion( apiVersions, configurations, models );

            return models;
        }
    }
}