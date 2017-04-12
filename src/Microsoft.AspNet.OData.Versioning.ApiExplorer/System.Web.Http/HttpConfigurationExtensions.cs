namespace System.Web.Http
{
    using Microsoft;
    using Microsoft.Web.Http.Description;
    using System.Diagnostics.Contracts;
    using System.Web.Http.Description;

    /// <summary>
    /// Provides extension methods for the <see cref="HttpConfiguration"/> class.
    /// </summary>
    public static class HttpConfigurationExtensions
    {
        /// <summary>
        /// Adds or replaces the configured <see cref="IApiExplorer">API explorer</see> with an implementation that supports OData and API versioning.
        /// </summary>
        /// <param name="configuration">The <see cref="HttpConfiguration">configuration</see> used to add the API explorer.</param>
        /// <returns>The newly registered <see cref="ODataApiExplorer">versioned OData API explorer</see>.</returns>
        /// <remarks>This method always replaces the <see cref="IApiExplorer"/> with a new instance of <see cref="ODataApiExplorer"/>. This method also
        /// configures the <see cref="ODataApiExplorer"/> to not use <see cref="ApiExplorerSettingsAttribute"/>, which enables exploring all OData
        /// controllers without additional configuration.</remarks>
        public static ODataApiExplorer AddODataApiExplorer( this HttpConfiguration configuration ) => configuration.AddODataApiExplorer( useApiExplorerSettings: false );

        /// <summary>
        /// Adds or replaces the configured <see cref="IApiExplorer">API explorer</see> with an implementation that supports OData and API versioning.
        /// </summary>
        /// <param name="configuration">The <see cref="HttpConfiguration">configuration</see> used to add the API explorer.</param>
        /// <param name="useApiExplorerSettings">Indicates whether the <see cref="ODataApiExplorer">OData API explorer</see> will use the
        /// <see cref="ApiExplorerSettingsAttribute"/> when present.</param>
        /// <returns>The newly registered <see cref="ODataApiExplorer">versioned API explorer</see>.</returns>
        /// <remarks>This method always replaces the <see cref="IApiExplorer"/> with a new instance of <see cref="ODataApiExplorer"/>.</remarks>
        public static ODataApiExplorer AddODataApiExplorer( this HttpConfiguration configuration, bool useApiExplorerSettings )
        {
            Arg.NotNull( configuration, nameof( configuration ) );
            Contract.Ensures( Contract.Result<ODataApiExplorer>() != null );

            var apiExplorer = new ODataApiExplorer( configuration ) { UseApiExplorerSettings = useApiExplorerSettings };
            configuration.Services.Replace( typeof( IApiExplorer ), apiExplorer );
            return apiExplorer;
        }
    }
}