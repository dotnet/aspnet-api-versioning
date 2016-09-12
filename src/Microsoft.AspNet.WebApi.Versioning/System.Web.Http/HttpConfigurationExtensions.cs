namespace System.Web.Http
{
    using Controllers;
    using Diagnostics.CodeAnalysis;
    using Diagnostics.Contracts;
    using Dispatcher;
    using Microsoft;
    using Microsoft.Web.Http;
    using Microsoft.Web.Http.Controllers;
    using Microsoft.Web.Http.Dispatcher;
    using Microsoft.Web.Http.Versioning;

    /// <summary>
    /// Provides extension methods for the <see cref="HttpConfiguration"/> class.
    /// </summary>
    public static class HttpConfigurationExtensions
    {
        private const string ApiVersioningOptionsKey = "MS_ApiVersioningOptions";

        /// <summary>
        /// Gets the current API versioning options.
        /// </summary>
        /// <param name="configuration">The current <see cref="HttpConfiguration">configuration</see>.</param>
        /// <returns>The current <see cref="ApiVersioningOptions">API versioning options</see>.</returns>
        public static ApiVersioningOptions GetApiVersioningOptions( this HttpConfiguration configuration )
        {
            Arg.NotNull( configuration, nameof( configuration ) );
            Contract.Ensures( Contract.Result<ApiVersioningOptions>() != null );

            var options = default( ApiVersioningOptions );
            return configuration.Properties.TryGetValue( ApiVersioningOptionsKey, out options ) ? options : new ApiVersioningOptions();
        }

        /// <summary>
        /// Adds service API versioning to the specified services collection.
        /// </summary>
        /// <param name="configuration">The <see cref="HttpConfiguration">configuration</see> that will use use service versioning.</param>
        public static void AddApiVersioning( this HttpConfiguration configuration ) => configuration.AddApiVersioning( _ => { } );

        /// <summary>
        /// Adds service API versioning to the specified services collection.
        /// </summary>
        /// <param name="configuration">The <see cref="HttpConfiguration">configuration</see> that will use use service versioning.</param>
        /// <param name="setupAction">An <see cref="Action{T}">action</see> used to configure the provided options.</param>
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated by a code contract." )]
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1", Justification = "Validated by a code contract." )]
        public static void AddApiVersioning( this HttpConfiguration configuration, Action<ApiVersioningOptions> setupAction )
        {
            Arg.NotNull( configuration, nameof( configuration ) );
            Arg.NotNull( setupAction, nameof( setupAction ) );

            var options = new ApiVersioningOptions();
            var services = configuration.Services;

            setupAction( options );
            services.Replace( typeof( IHttpControllerSelector ), new ApiVersionControllerSelector( configuration, options ) );
            services.Replace( typeof( IHttpActionSelector ), new ApiVersionActionSelector() );

            if ( options.ReportApiVersions )
            {
                configuration.Filters.Add( new ReportApiVersionsAttribute() );
            }

            configuration.Properties.AddOrUpdate( ApiVersioningOptionsKey, options, ( key, oldValue ) => options );
        }
    }
}