namespace Microsoft.Web.OData.Controllers
{
    using Http;
    using Http.Versioning;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.OData;
    using static Microsoft.OData.Core.ODataConstants;
    using static Microsoft.OData.Core.ODataUtils;
    using static Microsoft.OData.Core.ODataVersion;
    using static System.Net.HttpStatusCode;
    using static System.String;

    /// <summary>
    /// Represents a <see cref="ApiController">controller</see> for generating versioned OData service and metadata documents.
    /// </summary>
    /// <remarks>This controller is, itself, <see cref="ApiVersionNeutralAttribute">API version-neutral</see>.</remarks>
    [ApiVersionNeutral]
    public class VersionedMetadataController : MetadataController
    {
        private sealed class DiscoveredApiVersions
        {
            private const string ValueSeparator = ", ";

            internal DiscoveredApiVersions( IEnumerable<ApiVersionModel> models )
            {
                Contract.Requires( models != null );

                var supported = new HashSet<ApiVersion>();
                var deprecated = new HashSet<ApiVersion>();

                foreach ( var model in models )
                {
                    foreach ( var version in model.SupportedApiVersions )
                    {
                        supported.Add( version );
                    }

                    foreach ( var version in model.DeprecatedApiVersions )
                    {
                        deprecated.Add( version );
                    }
                }

                if ( supported.Count > 0 )
                {
                    deprecated.ExceptWith( supported );
                    SupportedApiVersions = Join( ValueSeparator, supported.OrderBy( v => v ).Select( v => v.ToString() ) );
                }

                if ( deprecated.Count > 0 )
                {
                    DeprecatedApiVersions = Join( ValueSeparator, deprecated.OrderBy( v => v ).Select( v => v.ToString() ) );
                }
            }

            public string SupportedApiVersions { get; }

            public string DeprecatedApiVersions { get; }
        }

        private const string ApiSupportedVersions = "api-supported-versions";
        private const string ApiDeprecatedVersions = "api-deprecated-versions";
        private readonly Lazy<DiscoveredApiVersions> discovered;

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionedMetadataController"/> class.
        /// </summary>
        public VersionedMetadataController()
        {
            discovered = new Lazy<DiscoveredApiVersions>( () => new DiscoveredApiVersions( DiscoverODataApiVersions() ) );
        }

        /// <summary>
        /// Handles a request for the HTTP OPTIONS method.
        /// </summary>
        /// <returns>A <see cref="IHttpActionResult">result</see> containing the response to the request.</returns>
        /// <remarks>When a request is made with OPTIONS /$metadata, then this method will return the following
        /// HTTP headers:
        /// <list type="table">
        ///  <listheader>
        ///  <term>Header Name</term>
        ///  <description>Description</description>
        ///  </listheader>
        ///  <item>
        ///   <term>OData-Version</term>
        ///   <description>The OData version supported by the endpoint.</description>
        ///  </item>
        ///  <item>
        ///   <term>api-supported-versions</term>
        ///   <description>A comma-separated list of all supported API versions, if any.</description>
        ///  </item>
        ///  <item>
        ///   <term>api-deprecated-versions</term>
        ///   <description>A comma-separated list of all supported API versions, if any.</description>
        ///  </item>
        /// </list>
        /// </remarks>
        [HttpOptions]
        public virtual IHttpActionResult GetOptions()
        {
            var response = new HttpResponseMessage( OK );
            var headers = response.Headers;

            response.Content = new StringContent( Empty );
            response.Content.Headers.Add( "Allow", new[] { "GET", "OPTIONS" } );
            response.Content.Headers.ContentType = null;
            headers.Add( ODataVersionHeader, ODataVersionToString( V4 ) );
            ReportApiVersions( headers );

            return ResponseMessage( response );
        }

        private DiscoveredApiVersions Discovered => discovered.Value;

        private void ReportApiVersions( HttpHeaders headers )
        {
            Contract.Requires( headers != null );

            var value = Discovered.SupportedApiVersions;

            if ( value != null )
            {
                headers.Add( ApiSupportedVersions, value );
            }

            value = Discovered.DeprecatedApiVersions;

            if ( value != null )
            {
                headers.Add( ApiDeprecatedVersions, value );
            }
        }

        private IEnumerable<ApiVersionModel> DiscoverODataApiVersions()
        {
            Contract.Ensures( Contract.Result<IEnumerable<ApiVersionModel>>() != null );

            var services = Configuration.Services;
            var assembliesResolver = services.GetAssembliesResolver();
            var typeResolver = services.GetHttpControllerTypeResolver();
            var controllerTypes = typeResolver.GetControllerTypes( assembliesResolver );

            return from controllerType in controllerTypes
                   where controllerType.IsODataController()
                   let descriptor = new HttpControllerDescriptor( Configuration, string.Empty, controllerType )
                   select descriptor.GetApiVersionModel();
        }
    }
}