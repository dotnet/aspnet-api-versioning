using System.Diagnostics.Contracts;
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
    using Microsoft.Web.Http.Routing;
    using Microsoft.Web.Http.Versioning;
    using Routing;

    /// <summary>
    /// Provides extension methods for the <see cref="HttpConfiguration"/> class.
    /// </summary>
    public static class HttpConfigurationExtensions
    {
        private const string ApiVersioningOptionsKey = "MS_ApiVersioningOptions";

        private static ApiVersioningOptions GetApiVersioningOptions( this HttpConfiguration configuration )
        {
            Contract.Requires( configuration != null );

            var options = default( ApiVersioningOptions );
            configuration.Properties.TryGetValue( ApiVersioningOptionsKey, out options );
            return options;
        }

        /// <summary>
        /// Gets the configured, default service API version.
        /// </summary>
        /// <param name="configuration">The current <see cref="HttpConfiguration">configuration</see>.</param>
        /// <returns>The configured, default <see cref="ApiVersion">API version</see>.</returns>
        /// <remarks>If the <paramref name="configuration"/> has not added API versioning, then this method
        /// always returns <see cref="ApiVersion.Default"/>.</remarks>
        public static ApiVersion GetDefaultApiVersion( this HttpConfiguration configuration )
        {
            Arg.NotNull( configuration, nameof( configuration ) );
            Contract.Ensures( Contract.Result<ApiVersion>() != null );

            return configuration.GetApiVersioningOptions()?.DefaultApiVersion ?? ApiVersion.Default;
        }

        /// <summary>
        /// Gets the configured service API version reader.
        /// </summary>
        /// <param name="configuration">The current <see cref="HttpConfiguration">configuration</see>.</param>
        /// <returns>The configured <see cref="IApiVersionReader">API service reader</see>.</returns>
        /// <remarks>If the <paramref name="configuration"/> has not added API versioning, then this method
        /// always returns a new instance of the <see cref="QueryStringApiVersionReader"/>.</remarks>
        public static IApiVersionReader GetApiVersionReader( this HttpConfiguration configuration )
        {
            Arg.NotNull( configuration, nameof( configuration ) );
            Contract.Ensures( Contract.Result<IApiVersionReader>() != null );

            return configuration.GetApiVersioningOptions()?.ApiVersionReader ?? new QueryStringApiVersionReader();
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

            setupAction( options );
            configuration.Services.Replace( typeof( IHttpControllerSelector ), new ApiVersionControllerSelector( configuration, options ) );
            configuration.Services.Replace( typeof( IHttpActionSelector ), new ApiVersionActionSelector() );

            if ( options.ReportApiVersions )
            {
                configuration.Filters.Add( new ReportApiVersionsAttribute() );
            }

            configuration.Properties.AddOrUpdate( ApiVersioningOptionsKey, options, ( key, oldValue ) => options );
        }
    }
}
