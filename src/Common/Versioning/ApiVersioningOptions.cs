#if WEBAPI
namespace Microsoft.Web.Http.Versioning
#else
namespace Microsoft.AspNetCore.Mvc.Versioning
#endif
{
    using Conventions;
    using System;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Represents the possible API versioning options for services.
    /// </summary>
    public partial class ApiVersioningOptions
    {
        private ApiVersion defaultApiVersion = ApiVersion.Default;
        private IApiVersionReader apiVersionReader = new QueryStringApiVersionReader();
        private IApiVersionSelector apiVersionSelector;
        private ApiVersionConventionBuilder conventions = new ApiVersionConventionBuilder();

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersioningOptions"/> class.
        /// </summary>
        public ApiVersioningOptions()
        {
            apiVersionSelector = new DefaultApiVersionSelector( this );
        }

        /// <summary>
        /// Gets or sets a value indicating whether requests report the service API version compatibility
        /// information in responses.
        /// </summary>
        /// <value>True if the responses contain service API version compatibility information; otherwise,
        /// false. The default value is <c>false</c>.</value>
        /// <remarks>
        /// <para>When this property is set to <c>true</c>, the HTTP headers "api-supported-versions" and
        /// "api-deprecated-versions" will be added to all valid service routes. This information is useful
        /// for advertising which versions are supported and scheduled for deprecation to clients. This
        /// information is also useful when supporting the OPTIONS verb.</para>
        /// <para>By setting this property to <c>true</c>, the <see cref="ReportApiVersionsAttribute"/> will
        /// be added a global action filter. To enable more granular control over when service API versions
        /// are reported, apply the <see cref="ReportApiVersionsAttribute"/> on specific controllers or
        /// controller actions.</para>
        /// </remarks>
        public bool ReportApiVersions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a default version is assumed when a client does
        /// does not provide a service API version.
        /// </summary>
        /// <value>True if the a default API version should be assumed when a client does not
        /// provide a service API version; otherwise, false. The default value is <c>false</c>.</value>
        /// <remarks>When a default API version is assumed, the version used is based up the
        /// result of the <see cref="IApiVersionSelector.SelectVersion"/> method.</remarks>
        public bool AssumeDefaultVersionWhenUnspecified { get; set; }

        /// <summary>
        /// Gets or sets the default API version applied to services that do not have explicit versions.
        /// </summary>
        /// <value>The default <see cref="ApiVersion">API version</see>. The default value is <see cref="ApiVersion.Default"/>.</value>
        public ApiVersion DefaultApiVersion
        {
            get
            {
                return defaultApiVersion;
            }
            set
            {
                Arg.NotNull( value, nameof( value ) );
                defaultApiVersion = value;
            }
        }

        /// <summary>
        /// Gets or sets the service API version reader.
        /// </summary>
        /// <value>An <see cref="IApiVersionReader">API version reader</see> object. The default value
        /// is an instance of the <see cref="QueryStringApiVersionReader"/>.</value>
        /// <remarks>The <see cref="IApiVersionReader">API version reader</see> is used to read the
        /// service API version specified by a client. The default value is the
        /// <see cref="QueryStringApiVersionReader"/>, which only reads the service API version from
        /// the "api-version" query string parameter. Replace the default value with an alternate
        /// implementation, such as the <see cref="QueryStringOrHeaderApiVersionReader"/>, which
        /// can read the service API version from additional information like HTTP headers.</remarks>
#if !WEBAPI
        [CLSCompliant( false )]
#endif
        public IApiVersionReader ApiVersionReader
        {
            get
            {
                Contract.Ensures( apiVersionReader != null );
                return apiVersionReader;
            }
            set
            {
                Arg.NotNull( value, nameof( value ) );
                apiVersionReader = value;
            }
        }

        /// <summary>
        /// Gets or sets the service API version selector.
        /// </summary>
        /// <value>An <see cref="IApiVersionSelector">API version selector</see> object.
        /// The default value is an instance of the <see cref="DefaultApiVersionSelector"/>.</value>
        /// <remarks>The <see cref="IApiVersionSelector">API version selector</see> is used to select
        /// an appropriate API version when a client does not specify a version. The default value is the
        /// <see cref="DefaultApiVersionSelector"/>, which always selects the <see cref="DefaultApiVersion"/>.</remarks>
#if !WEBAPI
        [CLSCompliant( false )]
#endif
        public IApiVersionSelector ApiVersionSelector
        {
            get
            {
                Contract.Ensures( apiVersionSelector != null );
                return apiVersionSelector;
            }
            set
            {
                Arg.NotNull( value, nameof( value ) );
                apiVersionSelector = value;
            }
        }

        /// <summary>
        /// Gets or sets the builder used to define API version conventions.
        /// </summary>
        /// <value>An <see cref="ApiVersionConventionBuilder">API version convention builder</see>.</value>
#if !WEBAPI
        [CLSCompliant( false )]
#endif
        public ApiVersionConventionBuilder Conventions
        {
            get
            {
                Contract.Ensures( conventions != null );
                return conventions;
            }
            set
            {
                Arg.NotNull( conventions, nameof( conventions ) );
                conventions = value;
            }
        }
    }
}