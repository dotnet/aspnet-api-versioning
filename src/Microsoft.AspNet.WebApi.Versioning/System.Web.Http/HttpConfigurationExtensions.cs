namespace System.Web.Http
{
    using Microsoft.Web.Http;
    using Microsoft.Web.Http.Controllers;
    using Microsoft.Web.Http.Dispatcher;
    using Microsoft.Web.Http.Versioning;
    using System;
    using System.Web.Http.Controllers;
    using System.Web.Http.Dispatcher;

    /// <summary>
    /// Provides extension methods for the <see cref="HttpConfiguration"/> class.
    /// </summary>
    public static class HttpConfigurationExtensions
    {
        const string ApiVersioningOptionsKey = "MS_ApiVersioningOptions";

        /// <summary>
        /// Gets the current API versioning options.
        /// </summary>
        /// <param name="configuration">The current <see cref="HttpConfiguration">configuration</see>.</param>
        /// <returns>The current <see cref="ApiVersioningOptions">API versioning options</see>.</returns>
        public static ApiVersioningOptions GetApiVersioningOptions( this HttpConfiguration configuration )
        {
            if ( configuration == null )
            {
                throw new ArgumentNullException( nameof( configuration ) );
            }

            return (ApiVersioningOptions) configuration.Properties.GetOrAdd( ApiVersioningOptionsKey, key => new ApiVersioningOptions() );
        }

        /// <summary>
        /// Adds service API versioning to the specified services collection.
        /// </summary>
        /// <param name="configuration">The <see cref="HttpConfiguration">configuration</see> that will use service versioning.</param>
        public static void AddApiVersioning( this HttpConfiguration configuration ) => configuration.AddApiVersioning( default );

        /// <summary>
        /// Adds service API versioning to the specified services collection.
        /// </summary>
        /// <param name="configuration">The <see cref="HttpConfiguration">configuration</see> that will use service versioning.</param>
        /// <param name="setupAction">An <see cref="Action{T}">action</see> used to configure the provided options.</param>
        public static void AddApiVersioning( this HttpConfiguration configuration, Action<ApiVersioningOptions>? setupAction )
        {
            if ( configuration == null )
            {
                throw new ArgumentNullException( nameof( configuration ) );
            }

            var options = new ApiVersioningOptions();
            var services = configuration.Services;

            setupAction?.Invoke( options );
            services.Replace( typeof( IHttpControllerSelector ), new ApiVersionControllerSelector( configuration, options ) );
            services.Replace( typeof( IHttpActionSelector ), new ApiVersionActionSelector() );

            if ( options.ReportApiVersions )
            {
                configuration.Filters.Add( new ReportApiVersionsAttribute() );
            }

            if ( options.ApiVersionReader.VersionsByMediaType() )
            {
                configuration.Filters.Add( new ApplyContentTypeVersionActionFilter( options.ApiVersionReader ) );
            }

            configuration.Properties.AddOrUpdate( ApiVersioningOptionsKey, options, ( key, oldValue ) => options );
            configuration.ParameterBindingRules.Add( typeof( ApiVersion ), ApiVersionParameterBinding.Create );
        }
    }
}