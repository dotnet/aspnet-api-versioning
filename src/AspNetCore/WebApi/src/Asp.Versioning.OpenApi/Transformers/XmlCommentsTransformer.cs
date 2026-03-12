// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OpenApi.Transformers;

using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using static System.Reflection.BindingFlags;

/// <summary>
/// Represents a <see cref="IOpenApiDocumentTransformer">transformer</see> used to apply XML comments to an
/// OpenAPI document.
/// </summary>
[CLSCompliant( false )]
public class XmlCommentsTransformer : IOpenApiSchemaTransformer, IOpenApiOperationTransformer
{
    internal XmlCommentsTransformer( XmlCommentsFile file ) :
        this( file.Path )
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XmlCommentsTransformer"/> class.
    /// </summary>
    /// <param name="path">The file path of the XML commands file.</param>
    public XmlCommentsTransformer( string path ) => Documentation = XmlComments.FromFile( path );

    internal bool IsEmpty => Documentation.IsEmpty;

    /// <summary>
    /// Gets the documentation associated with the transformer.
    /// </summary>
    protected XmlComments Documentation { get; }

    /// <inheritdoc />
    public virtual Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken )
    {
        ArgumentNullException.ThrowIfNull( schema );
        ArgumentNullException.ThrowIfNull( context );

        if ( context.JsonTypeInfo?.Type is not Type type )
        {
            return Task.CompletedTask;
        }

        var description = schema.Description;

        if ( string.IsNullOrEmpty( description )
             && !string.IsNullOrEmpty( description = Documentation.GetSummary( type ) ) )
        {
            schema.Description = description;
        }

        if ( schema.Example is null && ToJson( Documentation.GetExample( type ) ) is { } example )
        {
            schema.Example = example;
        }

        if ( schema.Properties is not { } properties )
        {
            return Task.CompletedTask;
        }

        foreach ( var (name, prop) in properties )
        {
            if ( prop is not null
                 && type.GetProperty( name, IgnoreCase | Instance | Public ) is { } property )
            {
                if ( string.IsNullOrEmpty( prop.Description )
                     && !string.IsNullOrEmpty( description = Documentation.GetSummary( property ) ) )
                {
                    prop.Description = description;
                }

                if ( prop.Example is null
                     && prop.Examples is not null
                     && ( example = ToJson( Documentation.GetExample( property ) ) ) is not null )
                {
                    prop.Examples.Add( example );
                }
            }
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public virtual Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken )
    {
        ArgumentNullException.ThrowIfNull( operation );
        ArgumentNullException.ThrowIfNull( context );

        if ( !TryResolveMethod( context.Description.ActionDescriptor, out var method ) )
        {
            return Task.CompletedTask;
        }

        if ( string.IsNullOrEmpty( operation.Summary ) )
        {
            operation.Summary = Documentation.GetSummary( method );
        }

        var description = operation.Description;

        if ( string.IsNullOrEmpty( description )
             && !string.IsNullOrEmpty( description = Documentation.GetDescription( method ) ) )
        {
            operation.Description = description;
        }

        if ( operation.Responses is { } responses )
        {
            foreach ( var (statusCode, response) in responses )
            {
                description = Documentation.GetResponseDescription( method, statusCode );

                if ( !string.IsNullOrEmpty( description ) )
                {
                    response.Description = description;
                }
            }
        }

        var parameters = operation.Parameters;
        var args = context.Description.ParameterDescriptions;

        if ( parameters is null || parameters.Count == 0 || args.Count == 0 )
        {
            return Task.CompletedTask;
        }

        for ( var i = 0; i < parameters.Count; i++ )
        {
            var parameter = parameters[i];

            if ( string.IsNullOrEmpty( parameter.Name ) )
            {
                continue;
            }

            for ( var j = 0; j < args.Count; j++ )
            {
                var arg = args[j];

                if ( arg.Name != parameter.Name )
                {
                    continue;
                }

                var name = arg.ParameterDescriptor.Name;

                if ( string.IsNullOrEmpty( parameter.Description )
                     && !string.IsNullOrEmpty( description = Documentation.GetParameterDescription( method, name ) ) )
                {
                    parameter.Description = description;
                }

                if ( parameter is OpenApiParameter param )
                {
                    if ( param.Example is null
                         && ToJson( Documentation.GetParameterExample( method, name ) ) is { } example )
                    {
                        param.Example = example;
                    }

                    param.Deprecated |= Documentation.IsParameterDeprecated( method, name );
                }

                break;
            }
        }

        return Task.CompletedTask;
    }

    private static bool TryResolveMethod( ActionDescriptor action, [MaybeNullWhen( false )] out MethodInfo method )
    {
        if ( action is ControllerActionDescriptor controller )
        {
            method = controller.MethodInfo;
            return true;
        }
        else
        {
            var metadata = action.EndpointMetadata;

            for ( var i = 0; i < metadata.Count; i++ )
            {
                if ( ( method = metadata[i] as MethodInfo ) is not null )
                {
                    return true;
                }
            }
        }

        method = default;
        return false;
    }

    private static JsonNode? ToJson( string? example )
    {
        if ( string.IsNullOrEmpty( example ) )
        {
            return default;
        }

        try
        {
            return JsonNode.Parse( example );
        }
        catch ( JsonException )
        {
            return JsonNode.Parse( $"\"{example}\"" );
        }
    }
}