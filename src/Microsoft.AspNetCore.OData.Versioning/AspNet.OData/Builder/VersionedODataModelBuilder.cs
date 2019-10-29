namespace Microsoft.AspNet.OData.Builder
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.Extensions.Options;
    using System;
    using System.Collections.Generic;
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
            if ( modelConfigurations == null )
            {
                throw new ArgumentNullException( nameof( modelConfigurations ) );
            }

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
        /// Gets the API versions for all known OData routes.
        /// </summary>
        /// <returns>The <see cref="IReadOnlyList{T}">sequence</see> of <see cref="ApiVersion">API versions</see>
        /// for all known OData routes.</returns>
        protected virtual IReadOnlyList<ApiVersion> GetApiVersions()
        {
            var items = ActionDescriptorCollectionProvider.ActionDescriptors.Items;
            var supported = new HashSet<ApiVersion>();
            var deprecated = new HashSet<ApiVersion>();

            for ( var i = 0; i < items.Count; i++ )
            {
                var item = items[i];

                if ( !( item is ControllerActionDescriptor action ) || !action.ControllerTypeInfo.IsODataController() )
                {
                    continue;
                }

                var model = action.GetApiVersionModel();
                var versions = model.SupportedApiVersions;

                for ( var j = 0; j < versions.Count; j++ )
                {
                    supported.Add( versions[j] );
                }

                versions = model.DeprecatedApiVersions;

                for ( var j = 0; j < versions.Count; j++ )
                {
                    deprecated.Add( versions[j] );
                }
            }

            deprecated.ExceptWith( supported );

            if ( supported.Count == 0 && deprecated.Count == 0 )
            {
                supported.Add( Options.DefaultApiVersion );
            }

            return supported.Union( deprecated ).ToArray();
        }
    }
}