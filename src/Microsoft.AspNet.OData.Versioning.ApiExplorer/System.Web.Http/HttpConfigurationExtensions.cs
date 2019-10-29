namespace System.Web.Http
{
    using Microsoft;
    using Microsoft.OData;
    using Microsoft.Web.Http.Description;
    using System.Collections.Concurrent;
    using System.ComponentModel.DataAnnotations;
    using System.Web.Http.Description;
    using System.Web.Http.Routing;

    /// <summary>
    /// Provides extension methods for the <see cref="HttpConfiguration"/> class.
    /// </summary>
    public static class HttpConfigurationExtensions
    {
        const string RootContainerMappingsKey = "Microsoft.AspNet.OData.RootContainerMappingsKey";
        const string UrlKeyDelimiterKey = "Microsoft.AspNet.OData.UrlKeyDelimiterKey";

        /// <summary>
        /// Adds or replaces the configured <see cref="IApiExplorer">API explorer</see> with an implementation that supports OData and API versioning.
        /// </summary>
        /// <param name="configuration">The <see cref="HttpConfiguration">configuration</see> used to add the API explorer.</param>
        /// <returns>The newly registered <see cref="ODataApiExplorer">versioned OData API explorer</see>.</returns>
        /// <remarks>This method always replaces the <see cref="IApiExplorer"/> with a new instance of <see cref="ODataApiExplorer"/>. This method also
        /// configures the <see cref="ODataApiExplorer"/> to not use <see cref="ApiExplorerSettingsAttribute"/>, which enables exploring all OData
        /// controllers without additional configuration.</remarks>
        public static ODataApiExplorer AddODataApiExplorer( this HttpConfiguration configuration ) => configuration.AddODataApiExplorer( default );

        /// <summary>
        /// Adds or replaces the configured <see cref="IApiExplorer">API explorer</see> with an implementation that supports OData and API versioning.
        /// </summary>
        /// <param name="configuration">The <see cref="HttpConfiguration">configuration</see> used to add the API explorer.</param>
        /// <param name="setupAction">An <see cref="Action{T}">action</see> used to configure the provided options.</param>
        /// <returns>The newly registered <see cref="ODataApiExplorer">versioned API explorer</see>.</returns>
        /// <remarks>This method always replaces the <see cref="IApiExplorer"/> with a new instance of <see cref="ODataApiExplorer"/>.</remarks>
        public static ODataApiExplorer AddODataApiExplorer( this HttpConfiguration configuration, Action<ODataApiExplorerOptions>? setupAction )
        {
            var options = new ODataApiExplorerOptions( configuration );

            setupAction?.Invoke( options );

            var apiExplorer = new ODataApiExplorer( configuration, options );
            configuration.Services.Replace( typeof( IApiExplorer ), apiExplorer );
            return apiExplorer;
        }

        internal static IServiceProvider GetODataRootContainer( this HttpConfiguration configuration, IHttpRoute route )
        {
            var containers = (ConcurrentDictionary<string, IServiceProvider>) configuration.Properties.GetOrAdd( RootContainerMappingsKey, key => new ConcurrentDictionary<string, IServiceProvider>() );
            var routeName = configuration.Routes.GetRouteName( route );

            if ( !string.IsNullOrEmpty( routeName ) && containers.TryGetValue( routeName!, out var serviceProvider ) )
            {
                return serviceProvider;
            }

            throw new InvalidOperationException( SR.NullContainer );
        }

        internal static ODataUrlKeyDelimiter? GetUrlKeyDelimiter( this HttpConfiguration configuration )
        {
            if ( configuration.Properties.TryGetValue( UrlKeyDelimiterKey, out var value ) )
            {
                return value as ODataUrlKeyDelimiter;
            }

            return default;
        }
    }
}