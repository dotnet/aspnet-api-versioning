namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Microsoft.Extensions.Options;
    using System;

    /// <summary>
    /// Represents the API versioning configuration for ASP.NET Core <see cref="MvcOptions">MVC options</see>.
    /// </summary>
    [CLSCompliant( false )]
    public class ApiVersioningMvcOptionsSetup : IPostConfigureOptions<MvcOptions>
    {
        readonly IOptions<ApiVersioningOptions> versioningOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersioningMvcOptionsSetup"/> class.
        /// </summary>
        /// <param name="options">The <see cref="ApiVersioningOptions">API versioning options</see> used to configure the MVC options.</param>
        public ApiVersioningMvcOptionsSetup( IOptions<ApiVersioningOptions> options ) => versioningOptions = options;

        /// <inheritdoc />
        public virtual void PostConfigure( string name, MvcOptions options )
        {
            var value = versioningOptions.Value;

            if ( value.ReportApiVersions )
            {
                options.Filters.AddService<ReportApiVersionsAttribute>();
            }

            options.ModelBinderProviders.Insert( 0, new ApiVersionModelBinderProvider() );
        }
    }
}