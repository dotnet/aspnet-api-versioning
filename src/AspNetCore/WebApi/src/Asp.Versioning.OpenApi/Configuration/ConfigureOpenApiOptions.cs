// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable CA1812

namespace Asp.Versioning.OpenApi;

using Asp.Versioning.ApiExplorer;
using Asp.Versioning.OpenApi.Reflection;
using Asp.Versioning.OpenApi.Transformers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

internal sealed class ConfigureOpenApiOptions(
    XmlCommentsFile file,
    IApiVersionDescriptionProvider provider,
    IOptions<OpenApiDocumentDescriptionOptions> descriptionOptions,
    [FromKeyedServices( typeof( ApiVersion ) )] Action<ApiVersionDescription, OpenApiOptions> configure )
    : IConfigureNamedOptions<OpenApiOptions>
{
    public void Configure( string? name, OpenApiOptions options )
    {
        var comparer = StringComparer.OrdinalIgnoreCase;
        var descriptions = provider.ApiVersionDescriptions;
        var xmlComments = new XmlCommentsTransformer( file );

        for ( var i = 0; i < descriptions.Count; i++ )
        {
            var description = descriptions[i];

            if ( !comparer.Equals( name, description.GroupName ) )
            {
                continue;
            }

            var apiExplorer = new ApiExplorerTransformer( description, descriptionOptions );

            options.SetDocumentName( description.GroupName );
            options.AddDocumentTransformer( apiExplorer );
            options.AddSchemaTransformer( apiExplorer );
            options.AddOperationTransformer( apiExplorer );

            if ( !xmlComments.IsEmpty )
            {
                options.AddSchemaTransformer( xmlComments );
                options.AddOperationTransformer( xmlComments );
            }

            configure( description, options );
            break;
        }
    }

    public void Configure( OpenApiOptions options )
    {
        // intentionally empty; all options must be named
    }
}