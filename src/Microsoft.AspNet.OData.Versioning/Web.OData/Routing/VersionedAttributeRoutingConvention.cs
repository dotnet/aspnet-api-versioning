namespace Microsoft.Web.OData.Routing
{
    using Http;
    using Microsoft.OData.Edm;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.OData.Routing;
    using System.Web.OData.Routing.Conventions;

    /// <summary>
    /// Represents an OData attribute routing convention with additional support for API versioning.
    /// </summary>
    public class VersionedAttributeRoutingConvention : AttributeRoutingConvention
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VersionedAttributeRoutingConvention"/> class.
        /// </summary>
        /// <param name="model">The <see cref="IEdmModel">EDM model</see> associated with the routing convention.</param>
        /// <param name="configuration">The current <see cref="HttpConfiguration">HTTP configuration</see>.</param>
        public VersionedAttributeRoutingConvention( IEdmModel model, HttpConfiguration configuration )
            : base( model, configuration )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionedAttributeRoutingConvention"/> class.
        /// </summary>
        /// <param name="model">The <see cref="IEdmModel">EDM model</see> associated with the routing convention.</param>
        /// <param name="configuration">The current <see cref="HttpConfiguration">HTTP configuration</see>.</param>
        /// <param name="pathTemplateHandler">The <see cref="IODataPathTemplateHandler">OData path template handler</see> associated with the routing convention.</param>
        public VersionedAttributeRoutingConvention( IEdmModel model, HttpConfiguration configuration, IODataPathTemplateHandler pathTemplateHandler )
            : base( model, configuration, pathTemplateHandler )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionedAttributeRoutingConvention"/> class.
        /// </summary>
        /// <param name="model">The <see cref="IEdmModel">EDM model</see> associated with the routing convention.</param>
        /// <param name="controllers">The <see cref="IEnumerable{T}">sequence</see> of <see cref="HttpControllerDescriptor">controller descriptors</see></param>
        public VersionedAttributeRoutingConvention( IEdmModel model, IEnumerable<HttpControllerDescriptor> controllers )
            : base( model, controllers )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionedAttributeRoutingConvention"/> class.
        /// </summary>
        /// <param name="model">The <see cref="IEdmModel">EDM model</see> associated with the routing convention.</param>
        /// <param name="controllers">The <see cref="IEnumerable{T}">sequence</see> of <see cref="HttpControllerDescriptor">controller descriptors</see>
        /// associated with the routing convention.</param>
        /// <param name="pathTemplateHandler">The <see cref="IODataPathTemplateHandler">OData path template handler</see> associated with the routing convention.</param>
        public VersionedAttributeRoutingConvention( IEdmModel model, IEnumerable<HttpControllerDescriptor> controllers, IODataPathTemplateHandler pathTemplateHandler )
            : base( model, controllers, pathTemplateHandler )
        {
        }

        /// <summary>
        /// Returns a value indicating whether the specified controller should be mapped using attribute routing conventions.
        /// </summary>
        /// <param name="controller">The <see cref="HttpControllerDescriptor">controller descriptor</see> to evaluate.</param>
        /// <returns>True if the <paramref name="controller"/> should be mapped as an OData controller; otherwise, false.</returns>
        /// <remarks>This method will match any OData controller that is API version-neutral or has a declared API version that
        /// matches the API version applied to the associated <see cref="P:Model">model</see>.</remarks>
        public override bool ShouldMapController( HttpControllerDescriptor controller )
        {
            Contract.Assume( controller != null );

            var versionModel = controller.GetApiVersionModel();

            if ( versionModel.IsApiVersionNeutral )
            {
                return true;
            }

            var apiVersion = Model.GetAnnotationValue<ApiVersionAnnotation>( Model )?.ApiVersion;

            return apiVersion != null && versionModel.DeclaredApiVersions.Contains( apiVersion );
        }
    }
}