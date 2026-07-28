// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OpenApi.Transformers;

using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Represents a <see cref="IOpenApiSchemaTransformer">transformer</see> used to describe well-known Protocol Buffers
/// types the way gRPC JSON transcoding serializes them.
/// </summary>
/// <remarks>
/// This transformer supports the following types:
///
/// <list type="bullet">
///  <item><c>Any</c></item>
///  <item><c>Duration</c></item>
///  <item><c>FieldMask</c></item>
///  <item><c>Timestamp</c></item>
/// </list>
///
/// These well-known types are messages, which are represented as complex types in Protocol Buffers, but are represented
/// as structured text via JSON transcoding. Each support type is represented with the applicable format, pattern, etc.
/// <c>Any</c> remains an object, but its members do not correspond to the fields of the message.
/// </remarks>
[CLSCompliant( false )]
public class GrpcWellKnownTypeSchemaTransformer : IOpenApiSchemaTransformer
{
    private const string Protobuf = "Google.Protobuf.WellKnownTypes.";

    private static readonly FrozenDictionary<string, Action<OpenApiSchema>> WellKnownTypes =
        new Dictionary<string, Action<OpenApiSchema>>( StringComparer.Ordinal )
        {
            [Protobuf + "Timestamp"] = AsDateTime,
            [Protobuf + "Duration"] = AsDuration,
            [Protobuf + "FieldMask"] = AsFieldMask,
            [Protobuf + "Any"] = AsAny,
        }
        .ToFrozenDictionary( StringComparer.Ordinal );

    /// <inheritdoc />
    public virtual Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken )
    {
        ArgumentNullException.ThrowIfNull( schema );
        ArgumentNullException.ThrowIfNull( context );

        if ( context.JsonTypeInfo?.Type.FullName is not string typeName
             || !WellKnownTypes.TryGetValue( typeName, out var describe ) )
        {
            return Task.CompletedTask;
        }

        // the message fields never apply; each supported type sets the JSON type it is actually serialized as
        schema.Properties?.Clear();
        schema.Required?.Clear();
        describe( schema );

        return Task.CompletedTask;
    }

    private static void AsDateTime( OpenApiSchema schema )
    {
        schema.Type = JsonSchemaType.String;
        schema.Format = "date-time";
    }

    // a Duration is serialized as the number of seconds with up to 9 fractional digits and an 's' suffix;
    // for example, '1.500s'. note that this is NOT the ISO 8601 duration that the OpenAPI 'duration' format
    // denotes, so applying that format would tell a client to send 'PT1.5S' and the request would fail.
    // describe the accepted shape with a pattern instead
    private static void AsDuration( OpenApiSchema schema )
    {
        schema.Type = JsonSchemaType.String;
        schema.Pattern = @"^-?(?:0|[1-9]\d*)(?:\.\d{1,9})?s$";
        schema.Example = JsonValue.Create( "1.500s" );
    }

    // a FieldMask is serialized as a comma-separated list of field paths. the paths are the JSON names of the
    // fields, so 'line_items' in the proto is 'lineItems' on the wire, and an unset mask is an empty string.
    // OpenAPI has no format for this so the shape is described with a pattern
    private static void AsFieldMask( OpenApiSchema schema )
    {
        const string Path = @"[a-z][a-zA-Z0-9]*(?:\.[a-z][a-zA-Z0-9]*)*";

        schema.Type = JsonSchemaType.String;
        schema.Pattern = $"^(?:{Path}(?:,{Path})*)?$";
        schema.Example = JsonValue.Create( "customer,lineItems" );
    }

    // an Any is the only supported type that remains an object, but its members are not the 'type_url' and 'value'
    // fields of the message. it is serialized as the contained message with an added '@type' member holding the type
    // URL; for example, { "@type": "type.googleapis.com/orders.Order", "id": 42 }. when the contained message has a
    // JSON mapping of its own, the mapped value is nested under 'value' instead; for example,
    // { "@type": "type.googleapis.com/google.protobuf.Duration", "value": "1.500s" }. the remaining members depend on
    // the contained message and cannot be known here, so only '@type' is described
    private static void AsAny( OpenApiSchema schema )
    {
        schema.Type = JsonSchemaType.Object;
        schema.AdditionalPropertiesAllowed = true;
        schema.Properties = new Dictionary<string, IOpenApiSchema>( StringComparer.Ordinal )
        {
            ["@type"] = new OpenApiSchema()
            {
                Type = JsonSchemaType.String,
                Description = "The fully-qualified type URL of the contained message.",
                Example = JsonValue.Create( "type.googleapis.com/google.protobuf.Duration" ),
            },
        };
    }
}