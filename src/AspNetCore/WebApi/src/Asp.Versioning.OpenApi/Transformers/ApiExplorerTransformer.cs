// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OpenApi.Transformers;

using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.OpenApi;
using System.Reflection;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ApiExplorerTransformer(
    ApiVersionDescription apiVersionDescription,
    IOptions<OpenApiDocumentDescriptionOptions> descriptionOptions ) :
    IOpenApiSchemaTransformer,
    IOpenApiDocumentTransformer,
    IOpenApiOperationTransformer
{
    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken )
    {
        if ( schema.Default is null
             && context.ParameterDescription?.DefaultValue is string value )
        {
            schema.Default = JsonNode.Parse( $"\"{value}\"" );
        }

        return Task.CompletedTask;
    }

    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken )
    {
        var options = descriptionOptions.Value;

        UpdateFromAssemblyInfo( document, apiVersionDescription );

        document.Info.Version = apiVersionDescription.ApiVersion.ToString();

        UpdateDescriptionToMarkdown( document, apiVersionDescription, options );
        AddLinkExtensions( document, apiVersionDescription );

        return Task.CompletedTask;
    }

    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken )
    {
        ArgumentNullException.ThrowIfNull( operation );
        ArgumentNullException.ThrowIfNull( context );

        operation.Deprecated |= context.Description.IsDeprecated;

        if ( operation.Parameters is not { } parameters )
        {
            return Task.CompletedTask;
        }

        var descriptions = context.Description.ParameterDescriptions;

        for ( var i = 0; i < descriptions.Count; i++ )
        {
            var description = descriptions[i];

            if ( description.ModelMetadata is not { } metadata
                || string.IsNullOrEmpty( metadata.Description ) )
            {
                continue;
            }

            for ( var j = 0; j < parameters.Count; j++ )
            {
                var parameter = parameters[j];

                if ( parameter.Name == description.Name
                     && string.IsNullOrEmpty( parameter.Description ) )
                {
                    parameter.Description = metadata.Description;
                }
            }
        }

        return Task.CompletedTask;
    }

    private static void UpdateFromAssemblyInfo( OpenApiDocument document, ApiVersionDescription api )
    {
        if ( Assembly.GetEntryAssembly() is not { } assembly )
        {
            return;
        }

        var title = assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title;
        var description = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description;

        if ( !string.IsNullOrEmpty( title ) )
        {
            document.Info.Title = $"{title} | {api.GroupName}";
        }

        if ( !string.IsNullOrEmpty( description ) )
        {
            document.Info.Description = description;
        }
    }

    private static void UpdateDescriptionToMarkdown(
        OpenApiDocument document,
        ApiVersionDescription api,
        OpenApiDocumentDescriptionOptions options )
    {
        var description = new StringBuilder( document.Info.Description );
        var links = new StringBuilder();

        if ( api.IsDeprecated )
        {
            var notice = options.DeprecationNotice();

            if ( !string.IsNullOrEmpty( notice ) )
            {
                if ( description[^1] != '.' )
                {
                    description.Append( '.' );
                }

                description.Append( ' ' ).Append( notice );
            }
        }

        if ( api.SunsetPolicy is { } sunset )
        {
            var notice = options.SunsetNotice( sunset );

            if ( !string.IsNullOrEmpty( notice ) )
            {
                if ( description[^1] != '.' )
                {
                    description.Append( '.' );
                }

                description.Append( ' ' ).Append( notice );
            }

            if ( !options.HidePolicyLinks && sunset.HasLinks )
            {
                AddMarkdownLinks( links, sunset.Links );
            }
        }

        if ( links.Length > 0 )
        {
            description.AppendLine()
                       .AppendLine()
                       .AppendLine( "### Links" )
                       .AppendLine()
                       .Append( links );
        }

        document.Info.Description = description.ToString();
    }

    private static bool IsHtml( LinkHeaderValue link ) =>
        StringSegmentComparer.OrdinalIgnoreCase.Equals( link.Type, "text/html" );

    private static void AddMarkdownLinks( StringBuilder markdown, IList<LinkHeaderValue> links )
    {
        for ( var i = 0; i < links.Count; i++ )
        {
            var link = links[i];

            if ( !IsHtml( link ) )
            {
                continue;
            }

            if ( StringSegment.IsNullOrEmpty( link.Title ) )
            {
                if ( link.LinkTarget.IsAbsoluteUri )
                {
                    markdown.Append( "- " ).AppendLine( link.LinkTarget.OriginalString );
                }
                else
                {
                    markdown.Append( "- <a href=\"" )
                            .Append( link.LinkTarget.OriginalString )
                            .Append( "\">" )
                            .Append( link.LinkTarget.OriginalString )
                            .AppendLine( "</a>" );
                }
            }
            else
            {
                markdown.Append( "- [" )
                        .Append( link.Title.ToString() )
                        .Append( "](" )
                        .Append( link.LinkTarget.OriginalString )
                        .Append( ')' )
                        .AppendLine();
            }
        }
    }

    private static void AddLinkExtensions( OpenApiDocument document, ApiVersionDescription api )
    {
        var array = new JsonArray();

        if ( api.SunsetPolicy is { } sunset && sunset.HasLinks )
        {
            AddLinks( array, sunset.Links );
        }

        if ( array.Count > 0 )
        {
            var extensions = document.Extensions ??= new Dictionary<string, IOpenApiExtension>();
            extensions["x-api-versioning"] = new JsonNodeExtension( array );
        }
    }

    [UnconditionalSuppressMessage( "ILLink", "IL2026" )]
    [UnconditionalSuppressMessage( "ILLink", "IL3050" )]
    private static void AddLinks( JsonArray array, IList<LinkHeaderValue> links )
    {
        for ( var i = 0; i < links.Count; i++ )
        {
            array.Add( LinkToJson( links[i] ) );
        }
    }

    private static JsonObject LinkToJson( LinkHeaderValue link )
    {
        var obj = new JsonObject();

        if ( link.Title.HasValue )
        {
            obj["title"] = link.Title.ToString();
        }

        if ( link.Type.HasValue )
        {
            obj["type"] = link.Type.ToString();
        }

        obj["rel"] = link.RelationType.ToString();
        obj["url"] = link.LinkTarget.ToString();

        if ( link.Media.HasValue )
        {
            obj["media"] = link.Media.ToString();
        }

        if ( link.Languages.Count > 0 )
        {
            obj["lang"] = new JsonArray( link.Languages.Select( l => JsonNode.Parse( l.ToString() ) ).ToArray() );
        }

        foreach ( var (key, value) in link.Extensions )
        {
            obj[key.ToString()] = value.ToString();
        }

        return obj;
    }
}