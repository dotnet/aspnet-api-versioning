namespace ApiVersioning.Examples;

using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text;

/// <summary>
/// Configures the Swagger generation options.
/// </summary>
/// <remarks>This allows API versioning to define a Swagger document per API version after the
/// <see cref="IApiVersionDescriptionProvider"/> service has been resolved from the service container.</remarks>
public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider provider;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigureSwaggerOptions"/> class.
    /// </summary>
    /// <param name="provider">The <see cref="IApiVersionDescriptionProvider">provider</see> used to generate Swagger documents.</param>
    public ConfigureSwaggerOptions( IApiVersionDescriptionProvider provider ) => this.provider = provider;

    /// <inheritdoc />
    public void Configure( SwaggerGenOptions options )
    {
        // add a swagger document for each discovered API version
        // note: you might choose to skip or document deprecated API versions differently
        foreach ( var description in provider.ApiVersionDescriptions )
        {
            options.SwaggerDoc( description.GroupName, CreateInfoForApiVersion( description ) );
        }
    }

    private static OpenApiInfo CreateInfoForApiVersion( ApiVersionDescription description )
    {
        var text = new StringBuilder( "An example application with OpenAPI, Swashbuckle, and API versioning." );
        var info = new OpenApiInfo()
        {
            Title = "Example API",
            Version = description.ApiVersion.ToString(),
            Contact = new OpenApiContact() { Name = "Bill Mei", Email = "bill.mei@somewhere.com" },
            License = new OpenApiLicense() { Name = "MIT", Url = new Uri( "https://opensource.org/licenses/MIT" ) }
        };

        if ( description.IsDeprecated )
        {
            text.Append( " This API version has been deprecated." );
        }

        if ( description.DeprecationPolicy is { } deprecationPolicy )
        {
            if ( deprecationPolicy.Date is { } when )
            {
                if ( when < DateTime.Now )
                {
                    text.Append( " The API has been deprecated on " )
                        .Append( when.Date.ToShortDateString() )
                        .Append( '.' );
                }
                else
                {
                    text.Append( " The API will be deprecated on " )
                        .Append( when.Date.ToShortDateString() )
                        .Append( '.' );
                }
            }

            if ( deprecationPolicy.HasLinks )
            {
                text.AppendLine();

                var rendered = false;

                foreach ( var link in deprecationPolicy.Links )
                {
                    if ( link.Type == "text/html" )
                    {
                        if ( !rendered )
                        {
                            text.Append( "<h4>Links</h4><ul>" );
                            rendered = true;
                        }

                        text.Append( "<li><a href=\"" );
                        text.Append( link.LinkTarget.OriginalString );
                        text.Append( "\">" );
                        text.Append(
                            StringSegment.IsNullOrEmpty( link.Title )
                            ? link.LinkTarget.OriginalString
                            : link.Title.ToString() );
                        text.Append( "</a></li>" );
                    }
                }

                if ( rendered )
                {
                    text.Append( "</ul>" );
                }
            }
        }

        if ( description.SunsetPolicy is { } sunsetPolicy )
        {
            if ( sunsetPolicy.Date is { } when )
            {
                if ( when < DateTime.Now )
                {
                    text.Append( " The API has been sunset on " )
                        .Append( when.Date.ToShortDateString() )
                        .Append( '.' );
                }
                else
                {
                    text.Append( " The API will be sunset on " )
                        .Append( when.Date.ToShortDateString() )
                        .Append( '.' );
                }
            }

            if ( sunsetPolicy.HasLinks )
            {
                text.AppendLine();

                var rendered = false;

                foreach ( var link in sunsetPolicy.Links )
                {
                    if ( link.Type == "text/html" )
                    {
                        if ( !rendered )
                        {
                            text.Append( "<h4>Links</h4><ul>" );
                            rendered = true;
                        }

                        text.Append( "<li><a href=\"" );
                        text.Append( link.LinkTarget.OriginalString );
                        text.Append( "\">" );
                        text.Append(
                            StringSegment.IsNullOrEmpty( link.Title )
                            ? link.LinkTarget.OriginalString
                            : link.Title.ToString() );
                        text.Append( "</a></li>" );
                    }
                }

                if ( rendered )
                {
                    text.Append( "</ul>" );
                }
            }
        }

        text.Append( "<h4>Additional Information</h4>" );
        info.Description = text.ToString();

        return info;
    }
}