namespace System.Web.Http
{
    using Microsoft;
    using Microsoft.OData.Core.UriParser;
    using Microsoft.Web.Http.Description;
    using System.Diagnostics.Contracts;
    using System.Web.Http.Description;
    using System.Web.OData.Extensions;
    using System.Web.OData.Routing;
    using static Linq.Expressions.Expression;

    /// <summary>
    /// Provides extension methods for the <see cref="HttpConfiguration"/> class.
    /// </summary>
    public static class HttpConfigurationExtensions
    {
        const string ResolverSettingsKey = "System.Web.OData.ResolverSettingsKey";
        const string ResolverSettingsTypeName = "System.Web.OData.ODataUriResolverSetttings";
        static readonly Lazy<Func<HttpConfiguration, bool>> getEnumPrefix = new Lazy<Func<HttpConfiguration, bool>>( CreateEnumPrefixFreeGetter );
        static readonly Lazy<Func<HttpConfiguration, ODataUrlConventions>> getUrlConventions = new Lazy<Func<HttpConfiguration, ODataUrlConventions>>( CreateUrlConventionsGetter );

        /// <summary>
        /// Adds or replaces the configured <see cref="IApiExplorer">API explorer</see> with an implementation that supports OData and API versioning.
        /// </summary>
        /// <param name="configuration">The <see cref="HttpConfiguration">configuration</see> used to add the API explorer.</param>
        /// <returns>The newly registered <see cref="ODataApiExplorer">versioned OData API explorer</see>.</returns>
        /// <remarks>This method always replaces the <see cref="IApiExplorer"/> with a new instance of <see cref="ODataApiExplorer"/>. This method also
        /// configures the <see cref="ODataApiExplorer"/> to not use <see cref="ApiExplorerSettingsAttribute"/>, which enables exploring all OData
        /// controllers without additional configuration.</remarks>
        public static ODataApiExplorer AddODataApiExplorer( this HttpConfiguration configuration ) => configuration.AddODataApiExplorer( _ => { } );

        /// <summary>
        /// Adds or replaces the configured <see cref="IApiExplorer">API explorer</see> with an implementation that supports OData and API versioning.
        /// </summary>
        /// <param name="configuration">The <see cref="HttpConfiguration">configuration</see> used to add the API explorer.</param>
        /// <param name="setupAction">An <see cref="Action{T}">action</see> used to configure the provided options.</param>
        /// <returns>The newly registered <see cref="ODataApiExplorer">versioned API explorer</see>.</returns>
        /// <remarks>This method always replaces the <see cref="IApiExplorer"/> with a new instance of <see cref="ODataApiExplorer"/>.</remarks>
        public static ODataApiExplorer AddODataApiExplorer( this HttpConfiguration configuration, Action<ODataApiExplorerOptions> setupAction )
        {
            Arg.NotNull( configuration, nameof( configuration ) );
            Arg.NotNull( setupAction, nameof( setupAction ) );
            Contract.Ensures( Contract.Result<ODataApiExplorer>() != null );

            var options = new ODataApiExplorerOptions( configuration );

            setupAction( options );

            var apiExplorer = new ODataApiExplorer( configuration, options );
            configuration.Services.Replace( typeof( IApiExplorer ), apiExplorer );
            return apiExplorer;
        }

        internal static ODataUrlConventions GetUrlKeyDelimiter( this HttpConfiguration configuration )
        {
            Contract.Requires( configuration != null );

            // REMARKS: this creates and populates the ODataUriResolverSetttings; OData URLs are case-sensitive by default.
            if ( !configuration.Properties.ContainsKey( ResolverSettingsKey ) )
            {
                configuration.EnableCaseInsensitive( false );
            }

            return getUrlConventions.Value( configuration );
        }

        internal static bool EnumPrefixFreeEnabled( this HttpConfiguration configuration )
        {
            Contract.Requires( configuration != null );

            // REMARKS: this creates and populates the ODataUriResolverSetttings; OData URLs are case-sensitive by default.
            if ( !configuration.Properties.ContainsKey( ResolverSettingsKey ) )
            {
                configuration.EnableCaseInsensitive( false );
            }

            return getEnumPrefix.Value( configuration );
        }

        /// <summary>
        /// Build: ((ODataUriResolverSetttings) HttpConfiguration.Properties["System.Web.OData.ResolverSettingsKey"]).UrlConventions
        /// </summary>
        /// <returns>A strongly-typed delegate.</returns>
        static Func<HttpConfiguration, ODataUrlConventions> CreateUrlConventionsGetter()
        {
            Contract.Ensures( Contract.Result<Func<HttpConfiguration, ODataUrlConventions>>() != null );
            return CreateODataUriResolverSetttingsGetter<ODataUrlConventions>( "UrlConventions" );
        }

        /// <summary>
        /// Build: ((ODataUriResolverSetttings) HttpConfiguration.Properties["System.Web.OData.ResolverSettingsKey"]).EnumPrefixFree
        /// </summary>
        /// <returns>A strongly-typed delegate.</returns>
        static Func<HttpConfiguration, bool> CreateEnumPrefixFreeGetter()
        {
            Contract.Ensures( Contract.Result<Func<HttpConfiguration, bool>>() != null );
            return CreateODataUriResolverSetttingsGetter<bool>( "EnumPrefixFree" );
        }

        static Func<HttpConfiguration, TValue> CreateODataUriResolverSetttingsGetter<TValue>( string propertyName )
        {
            Contract.Requires( !string.IsNullOrEmpty( propertyName ) );
            Contract.Ensures( Contract.Result<Func<HttpConfiguration, TValue>>() != null );

            var configurationType = typeof( HttpConfiguration );
            var resolverSettingsType = typeof( DefaultODataPathHandler ).Assembly.GetType( ResolverSettingsTypeName );
            var c = Parameter( configurationType, "c" );
            var property = Property( Property( c, "Properties" ), "Item", Constant( ResolverSettingsKey ) );
            var body = Property( Convert( property, resolverSettingsType ), propertyName );
            var lambda = Lambda<Func<HttpConfiguration, TValue>>( body, c );
            var func = lambda.Compile();

            return func;
        }
    }
}