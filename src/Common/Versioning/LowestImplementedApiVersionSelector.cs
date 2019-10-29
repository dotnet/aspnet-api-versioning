#if WEBAPI
namespace Microsoft.Web.Http.Versioning
#else
namespace Microsoft.AspNetCore.Mvc.Versioning
#endif
{
#if !WEBAPI
    using Microsoft.AspNetCore.Http;
#endif
    using System;
    using System.Linq;
#if WEBAPI
    using HttpRequest = System.Net.Http.HttpRequestMessage;
#endif

    /// <summary>
    /// Represents an <see cref="IApiVersionSelector">API version selector</see> which selects the lowest
    /// API version implemented by the requested service.
    /// </summary>
#if !WEBAPI
    [CLSCompliant( false )]
#endif
    public class LowestImplementedApiVersionSelector : IApiVersionSelector
    {
        readonly ApiVersioningOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="LowestImplementedApiVersionSelector"/> class.
        /// </summary>
        /// <param name="options">The <see cref="ApiVersioningOptions">API versioning options</see> associated with the selector.</param>
        public LowestImplementedApiVersionSelector( ApiVersioningOptions options ) => this.options = options;

        /// <summary>
        /// Selects an API version given the specified HTTP request and API version information.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequest">HTTP request</see> to select the version for.</param>
        /// <param name="model">The <see cref="ApiVersionModel">model</see> to select the version from.</param>
        /// <returns>The selected <see cref="ApiVersion">API version</see>.</returns>
        /// <remarks>This method always returns the default <see cref="ApiVersion.Default">API version</see>.</remarks>
        public virtual ApiVersion SelectVersion( HttpRequest request, ApiVersionModel model )
        {
            if ( model == null )
            {
                throw new ArgumentNullException( nameof( model ) );
            }

            switch ( model.ImplementedApiVersions.Count )
            {
                case 0:
                    return options.DefaultApiVersion;
                case 1:
                    var version = model.ImplementedApiVersions[0];
                    return version.Status == null ? version : options.DefaultApiVersion;
            }

            return model.ImplementedApiVersions.Where( v => v.Status == null ).Min( v => v ) ?? options.DefaultApiVersion;
        }
    }
}