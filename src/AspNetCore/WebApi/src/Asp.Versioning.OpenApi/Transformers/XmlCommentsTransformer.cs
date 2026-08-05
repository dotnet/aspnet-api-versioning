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
public class XmlCommentsTransformer : IOpenApiSchemaTransformer, IOpenApiOperationTransformer, IOpenApiDocumentTransformer
{
    private const string ModelTypeKey = "x-asp-versioning-model-type";

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

        // a schema is created once per type and the same instance is reused everywhere the type appears, including
        // as the target of a reference. describing a member here would write the description of one property onto
        // the schema of the property's own type, which every other use of that type would then report. the members
        // are described once the document is complete and the shared schemas have been replaced by references
        if ( schema.Properties is { Count: > 0 } )
        {
            ( schema.Metadata ??= new Dictionary<string, object>() )[ModelTypeKey] = type;
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public virtual Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken )
    {
        ArgumentNullException.ThrowIfNull( document );
        ArgumentNullException.ThrowIfNull( context );

        new OpenApiWalker( new SchemaVisitor( DescribeMembers ) ).Walk( document );

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

        if ( string.IsNullOrEmpty( description ) )
        {
            if ( string.IsNullOrEmpty( description = Documentation.GetRemarks( method ) ) )
            {
                description = Documentation.GetDescription( method );
            }

            if ( !string.IsNullOrEmpty( description ) )
            {
                operation.Description = description;
            }
        }

        if ( operation.Responses is { } responses )
        {
            // <returns> describes the return value of the method, which only maps to a response when there is
            // exactly one. with more than one, there is no way to know which response it refers to; <response>
            // exists for that case and always wins where both are present.
            var returns = responses.Count == 1 ? Documentation.GetReturns( method ) : string.Empty;

            foreach ( var (statusCode, response) in responses )
            {
                description = Documentation.GetResponseDescription( method, statusCode );

                if ( string.IsNullOrEmpty( description ) )
                {
                    description = returns;
                }

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

                if ( string.IsNullOrEmpty( parameter.Description ) )
                {
                    description = Documentation.GetParameterDescription( method, name );

                    if ( string.IsNullOrEmpty( description )
                         && arg.ParameterDescriptor is ControllerParameterDescriptor parameterDescriptor
                         && parameterDescriptor.ParameterInfo is { } parameterInfo )
                    {
                        description = Documentation.GetParameterDescription( parameterInfo );
                    }

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

    [UnconditionalSuppressMessage( "ILLink", "IL2070", Justification = "The model type is reported by the API Explorer and is never trimmed" )]
    private void DescribeMembers( OpenApiSchema schema )
    {
        if ( schema.Metadata is not { } metadata
             || !metadata.TryGetValue( ModelTypeKey, out var value )
             || value is not Type type
             || schema.Properties is not { } properties )
        {
            return;
        }

        foreach ( var (name, member) in properties )
        {
            if ( member is not null
                 && type.GetProperty( name, IgnoreCase | Instance | Public ) is { } property )
            {
                Describe( member, property );
            }
        }
    }

    private void Describe( IOpenApiSchema member, PropertyInfo property )
    {
        // a member whose type has a schema of its own is a reference to that schema, which is shared by every
        // other use of the type. the description belongs to the reference rather than to what it refers to;
        // OpenAPI 3.1 allows both to appear together
        if ( member is OpenApiSchemaReference reference )
        {
            if ( string.IsNullOrEmpty( reference.Reference.Description )
                 && GetPropertyDescription( property ) is { Length: > 0 } summary )
            {
                reference.Reference.Description = summary;
            }

            return;
        }

        if ( string.IsNullOrEmpty( member.Description )
             && GetPropertyDescription( property ) is { Length: > 0 } description )
        {
            member.Description = description;
        }

        if ( member.Example is null
             && member.Examples is not null
             && ToJson( Documentation.GetExample( property ) ) is { } example )
        {
            member.Examples.Add( example );
        }
    }

    // <summary> says what a property is and <value> says what it holds. they are complementary, so when a
    // property has both, both are used
    private string GetPropertyDescription( MemberInfo property )
    {
        var summary = Documentation.GetSummary( property );
        var value = Documentation.GetValue( property );

        if ( string.IsNullOrEmpty( value ) )
        {
            return summary;
        }

        return string.IsNullOrEmpty( summary ) ? value : summary + '\n' + value;
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

    // the same schema is reached once per use, so a schema is only described the first time it is visited.
    // describing it again would append a duplicate example
    private sealed class SchemaVisitor( Action<OpenApiSchema> describe ) : OpenApiVisitorBase
    {
        private readonly HashSet<object> visited = new( ReferenceEqualityComparer.Instance );

        public override void Visit( IOpenApiSchema schema )
        {
            if ( schema is OpenApiSchema described && visited.Add( described ) )
            {
                describe( described );
            }
        }
    }
}