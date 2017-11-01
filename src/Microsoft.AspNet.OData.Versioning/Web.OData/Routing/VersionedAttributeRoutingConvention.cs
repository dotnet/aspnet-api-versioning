namespace Microsoft.Web.OData.Routing
{
    using Microsoft.Web.Http;
    using Microsoft.Web.Http.Versioning;
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
        readonly ApiVersion apiVersion;

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionedAttributeRoutingConvention"/> class.
        /// </summary>
        /// <param name="routeName">The name of the route.</param>
        /// <param name="configuration">The current <see cref="HttpConfiguration">HTTP configuration</see>.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> associated with the convention.</param>
        public VersionedAttributeRoutingConvention( string routeName, HttpConfiguration configuration, ApiVersion apiVersion )
            : base( routeName, configuration )
        {
            Arg.NotNull( apiVersion, nameof( apiVersion ) );
            this.apiVersion = apiVersion;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionedAttributeRoutingConvention"/> class.
        /// </summary>
        /// <param name="routeName">The name of the route.</param>
        /// <param name="configuration">The current <see cref="HttpConfiguration">HTTP configuration</see>.</param>
        /// <param name="pathTemplateHandler">The <see cref="IODataPathTemplateHandler">OData path template handler</see> associated with the routing convention.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> associated with the convention.</param>
        public VersionedAttributeRoutingConvention( string routeName, HttpConfiguration configuration, IODataPathTemplateHandler pathTemplateHandler, ApiVersion apiVersion )
            : base( routeName, configuration, pathTemplateHandler )
        {
            Arg.NotNull( apiVersion, nameof( apiVersion ) );
            this.apiVersion = apiVersion;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionedAttributeRoutingConvention"/> class.
        /// </summary>
        /// <param name="routeName">The name of the route.</param>
        /// <param name="controllers">The <see cref="IEnumerable{T}">sequence</see> of <see cref="HttpControllerDescriptor">controller descriptors</see></param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> associated with the convention.</param>
        public VersionedAttributeRoutingConvention( string routeName, IEnumerable<HttpControllerDescriptor> controllers, ApiVersion apiVersion )
            : base( routeName, controllers )
        {
            Arg.NotNull( apiVersion, nameof( apiVersion ) );
            this.apiVersion = apiVersion;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionedAttributeRoutingConvention"/> class.
        /// </summary>
        /// <param name="routeName">The name of the route.</param>
        /// <param name="controllers">The <see cref="IEnumerable{T}">sequence</see> of <see cref="HttpControllerDescriptor">controller descriptors</see>
        /// associated with the routing convention.</param>
        /// <param name="pathTemplateHandler">The <see cref="IODataPathTemplateHandler">OData path template handler</see> associated with the routing convention.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> associated with the convention.</param>
        public VersionedAttributeRoutingConvention( string routeName, IEnumerable<HttpControllerDescriptor> controllers, IODataPathTemplateHandler pathTemplateHandler, ApiVersion apiVersion )
            : base( routeName, controllers, pathTemplateHandler )
        {
            Arg.NotNull( apiVersion, nameof( apiVersion ) );
            this.apiVersion = apiVersion;
        }

        /// <summary>
        /// Returns a value indicating whether the specified controller should be mapped using attribute routing conventions.
        /// </summary>
        /// <param name="controller">The <see cref="HttpControllerDescriptor">controller descriptor</see> to evaluate.</param>
        /// <returns>True if the <paramref name="controller"/> should be mapped as an OData controller; otherwise, false.</returns>
        /// <remarks>This method will match any OData controller that is API version-neutral or has a declared API version that
        /// matches the API version applied to the associated <see cref="ApiVersionModel">model</see>.</remarks>
        public override bool ShouldMapController( HttpControllerDescriptor controller )
        {
            Contract.Assume( controller != null );

            var versionModel = controller.GetApiVersionModel();

            if ( versionModel.IsApiVersionNeutral )
            {
                return true;
            }

            return apiVersion != null && versionModel.DeclaredApiVersions.Contains( apiVersion );
        }
    }
}