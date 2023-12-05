// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.Options;
using static Asp.Versioning.ApiVersionParameterLocation;

/// <summary>
/// Represents the API versioning configuration for ASP.NET Core <see cref="MvcOptions">MVC options</see>.
/// </summary>
[CLSCompliant( false )]
public class ApiVersioningMvcOptionsSetup : IPostConfigureOptions<MvcOptions>
{
    private readonly IOptions<ApiVersioningOptions> versioningOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersioningMvcOptionsSetup"/> class.
    /// </summary>
    /// <param name="options">The <see cref="ApiVersioningOptions">API versioning options</see> used to configure the MVC options.</param>
    public ApiVersioningMvcOptionsSetup( IOptions<ApiVersioningOptions> options ) => versioningOptions = options;

    /// <inheritdoc />
    public virtual void PostConfigure( string? name, MvcOptions options )
    {
        ArgumentNullException.ThrowIfNull( options );

        var value = versioningOptions.Value;

        if ( value.ReportApiVersions )
        {
            options.Filters.AddService<ReportApiVersionsAttribute>();
        }

        var reader = value.ApiVersionReader;

        if ( reader.VersionsByMediaType() )
        {
            var parameterName = reader.GetParameterName( MediaTypeParameter );

            if ( !string.IsNullOrEmpty( parameterName ) )
            {
                options.Filters.AddService<ApplyContentTypeVersionActionFilter>();
            }
        }

        var modelMetadataDetailsProviders = options.ModelMetadataDetailsProviders;

        modelMetadataDetailsProviders.Insert( 0, new SuppressChildValidationMetadataProvider( typeof( ApiVersion ) ) );
        modelMetadataDetailsProviders.Insert( 0, new BindingSourceMetadataProvider( typeof( ApiVersion ), BindingSource.Special ) );
        options.ModelBinderProviders.Insert( 0, new ApiVersionModelBinderProvider() );
    }
}