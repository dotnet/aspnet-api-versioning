namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
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
            if ( options == null )
            {
                throw new ArgumentNullException( nameof( options ) );
            }

            var value = versioningOptions.Value;

            if ( value.ReportApiVersions )
            {
                options.Filters.AddService<ReportApiVersionsAttribute>();
            }

            if ( value.ApiVersionReader.VersionsByMediaType() )
            {
                options.Filters.AddService<ApplyContentTypeVersionActionFilter>();
            }

            var modelMetadataDetailsProviders = options.ModelMetadataDetailsProviders;

            modelMetadataDetailsProviders.Insert( 0, new SuppressChildValidationMetadataProvider( typeof( ApiVersion ) ) );
            modelMetadataDetailsProviders.Insert( 0, new BindingSourceMetadataProvider( typeof( ApiVersion ), BindingSource.Special ) );
            options.ModelBinderProviders.Insert( 0, new ApiVersionModelBinderProvider() );
        }
    }
}