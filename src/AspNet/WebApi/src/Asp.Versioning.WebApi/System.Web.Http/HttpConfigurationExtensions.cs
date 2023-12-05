// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace System.Web.Http;

using Asp.Versioning;
using Asp.Versioning.Controllers;
using Asp.Versioning.Dependencies;
using Asp.Versioning.Dispatcher;
using Asp.Versioning.Formatting;
using Backport;
using System.Globalization;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using static Asp.Versioning.ApiVersionParameterLocation;

/// <summary>
/// Provides extension methods for the <see cref="HttpConfiguration"/> class.
/// </summary>
public static class HttpConfigurationExtensions
{
    private const string ApiVersioningServicesKey = "MS_ApiVersioningServices";

    /// <summary>
    /// Gets the current API versioning options.
    /// </summary>
    /// <param name="configuration">The current <see cref="HttpConfiguration">configuration</see>.</param>
    /// <returns>The current <see cref="ApiVersioningOptions">API versioning options</see>.</returns>
    public static ApiVersioningOptions GetApiVersioningOptions( this HttpConfiguration configuration )
    {
        ArgumentNullException.ThrowIfNull( configuration );
        return configuration.ApiVersioningServices().ApiVersioningOptions;
    }

    /// <summary>
    /// Converts problem details into error objects.
    /// </summary>
    /// <param name="configuration">The current <see cref="HttpConfiguration">configuration</see>.</param>
    /// <remarks>This enables backward compatibility by converting <see cref="ProblemDetails"/> into Error Objects that
    /// conform to the <a ref="https://github.com/microsoft/api-guidelines/blob/vNext/Guidelines.md#7102-error-condition-responses">Error Responses</a>
    /// in the Microsoft REST API Guidelines and
    /// <a ref="https://docs.oasis-open.org/odata/odata-json-format/v4.01/odata-json-format-v4.01.html#_Toc38457793">OData Error Responses</a>.</remarks>
    public static void ConvertProblemDetailsToErrorObject( this HttpConfiguration configuration )
    {
        ArgumentNullException.ThrowIfNull( configuration );
        configuration.Initializer += EnableErrorObjectResponses;
    }

    /// <summary>
    /// Adds service API versioning to the specified services collection.
    /// </summary>
    /// <param name="configuration">The <see cref="HttpConfiguration">configuration</see> that will use service versioning.</param>
    public static void AddApiVersioning( this HttpConfiguration configuration )
    {
        ArgumentNullException.ThrowIfNull( configuration );
        configuration.AddApiVersioning( new ApiVersioningOptions() );
    }

    /// <summary>
    /// Adds service API versioning to the specified services collection.
    /// </summary>
    /// <param name="configuration">The <see cref="HttpConfiguration">configuration</see> that will use service versioning.</param>
    /// <param name="setupAction">An <see cref="Action{T}">action</see> used to configure the provided options.</param>
    public static void AddApiVersioning( this HttpConfiguration configuration, Action<ApiVersioningOptions> setupAction )
    {
        ArgumentNullException.ThrowIfNull( configuration );
        ArgumentNullException.ThrowIfNull( setupAction );

        var options = new ApiVersioningOptions();

        setupAction( options );
        ValidateApiVersioningOptions( options );
        configuration.AddApiVersioning( options );
    }

    private static void AddApiVersioning( this HttpConfiguration configuration, ApiVersioningOptions options )
    {
        var services = configuration.Services;

        services.Replace( typeof( IHttpControllerSelector ), new ApiVersionControllerSelector( configuration, options ) );
        services.Replace( typeof( IHttpActionSelector ), new ApiVersionActionSelector() );

        if ( options.ReportApiVersions )
        {
            configuration.Filters.Add( new ReportApiVersionsAttribute() );
        }

        var reader = options.ApiVersionReader;

        if ( reader.VersionsByMediaType() )
        {
            var parameterName = reader.GetParameterName( MediaTypeParameter );

            if ( !string.IsNullOrEmpty( parameterName ) )
            {
                configuration.Filters.Add( new ApplyContentTypeVersionActionFilter( reader ) );
            }
        }

        configuration.ApiVersioningServices().ApiVersioningOptions = options;
        configuration.ParameterBindingRules.Add( typeof( ApiVersion ), ApiVersionParameterBinding.Create );
        configuration.Formatters.Insert( 0, new ProblemDetailsMediaTypeFormatter( configuration.Formatters.JsonFormatter ?? new() ) );
    }

    // ApiVersion.Neutral does not have the same meaning as IApiVersionNeutral. setting
    // ApiVersioningOptions.DefaultApiVersion this value will not make all APIs version-neutral
    // and will likely lead to many unexpected side effects. this is a best-effort, one-time
    // validation check to help prevent people from going off the rails. if someone bypasses
    // this validation by removing the check or updating the value later, then caveat emptor.
    //
    // REF: https://github.com/dotnet/aspnet-api-versioning/issues/1011
    private static void ValidateApiVersioningOptions( ApiVersioningOptions options )
    {
        if ( options.DefaultApiVersion == ApiVersion.Neutral )
        {
            var message = string.Format(
                CultureInfo.CurrentCulture,
                SR.InvalidDefaultApiVersion,
                nameof( ApiVersion ),
                nameof( ApiVersion.Neutral ),
                nameof( ApiVersioningOptions ),
                nameof( ApiVersioningOptions.DefaultApiVersion ),
                nameof( IApiVersionNeutral ) );

            throw new InvalidOperationException( message );
        }
    }

    private static void EnableErrorObjectResponses( HttpConfiguration configuration )
    {
        configuration.ApiVersioningServices().Replace(
            typeof( IProblemDetailsFactory ),
            static ( sc, t ) => new ErrorObjectFactory() );

        var formatters = configuration.Formatters;
        var problemDetails = ProblemDetailsMediaTypeFormatter.DefaultMediaType;

        for ( var i = 0; i < formatters.Count; i++ )
        {
            var mediaTypes = formatters[i].SupportedMediaTypes;

            for ( var j = 0; j < mediaTypes.Count; j++ )
            {
                if ( mediaTypes[j].Equals( problemDetails ) )
                {
                    formatters.RemoveAt( i );
                    return;
                }
            }
        }
    }

    internal static DefaultContainer ApiVersioningServices( this HttpConfiguration configuration ) =>
        (DefaultContainer) configuration.Properties.GetOrAdd( ApiVersioningServicesKey, key => new DefaultContainer() );
}