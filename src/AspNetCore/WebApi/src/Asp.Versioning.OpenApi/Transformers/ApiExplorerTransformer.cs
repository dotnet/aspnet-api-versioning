// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OpenApi.Transformers;

using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using System.Reflection;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ApiExplorerTransformer( ApiVersionDescription apiVersionDescription ) :
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
        if ( Assembly.GetEntryAssembly() is { } assembly )
        {
            var title = assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title;
            var description = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description;

            if ( !string.IsNullOrEmpty( title ) )
            {
                document.Info.Title = $"{title} | {apiVersionDescription.GroupName}";
            }

            if ( !string.IsNullOrEmpty( description ) )
            {
                document.Info.Summary = description;
            }
        }

        document.Info.Version = apiVersionDescription.ApiVersion.ToString();

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
}