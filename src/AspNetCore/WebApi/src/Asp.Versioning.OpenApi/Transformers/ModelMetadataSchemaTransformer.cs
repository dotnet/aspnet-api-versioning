// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OpenApi.Transformers;

using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using System.Collections.Frozen;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Represents a <see cref="IOpenApiSchemaTransformer">transformer</see> used to remove schema members that the
/// <see cref="ModelMetadata">model metadata</see> of the described API does not report.
/// </summary>
/// <remarks>
/// A model type is a single CLR type, but the members it exposes can differ by API version. An API description
/// provider that reports a reduced set of <see cref="ModelMetadata.Properties">properties</see> is describing a
/// subset of the type, which the schema generated from the CLR type alone cannot express. Only metadata that
/// reports the API version it was described for is considered authoritative; metadata describing a type as
/// declared is left untouched.
/// </remarks>
[CLSCompliant( false )]
public class ModelMetadataSchemaTransformer : IOpenApiSchemaTransformer
{
    private readonly IApiDescriptionGroupCollectionProvider provider;
    private readonly string groupName;
    private FrozenDictionary<Type, FrozenSet<string>>? members;

    /// <summary>
    /// Initializes a new instance of the <see cref="ModelMetadataSchemaTransformer"/> class.
    /// </summary>
    /// <param name="provider">The <see cref="IApiDescriptionGroupCollectionProvider">provider</see> used to
    /// enumerate the described APIs.</param>
    /// <param name="options">The <see cref="VersionedOpenApiOptions">options</see> applied
    /// to OpenAPI document descriptions.</param>
    public ModelMetadataSchemaTransformer( IApiDescriptionGroupCollectionProvider provider, VersionedOpenApiOptions options )
    {
        ArgumentNullException.ThrowIfNull( options );

        this.provider = provider;
        groupName = options.Description.GroupName;
    }

    /// <inheritdoc />
    public virtual Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken )
    {
        ArgumentNullException.ThrowIfNull( schema );
        ArgumentNullException.ThrowIfNull( context );

        // the transformer is applied to nested schemas and to response schemas, neither of which carry the
        // originating parameter description. the reported members are keyed by model type instead so that a
        // message is filtered the same way wherever it appears in the document
        if ( schema.Properties is not { Count: > 0 } properties ||
             context.JsonTypeInfo is not { } json ||
             !Members.TryGetValue( json.Type, out var included ) )
        {
            return Task.CompletedTask;
        }

        var jsonProperties = json.Properties;

        for ( var i = 0; i < jsonProperties.Count; i++ )
        {
            var jsonProperty = jsonProperties[i];

            if ( jsonProperty.AttributeProvider is PropertyInfo property && !included.Contains( property.Name ) )
            {
                properties.Remove( jsonProperty.Name );
                schema.Required?.Remove( jsonProperty.Name );
            }
        }

        return Task.CompletedTask;
    }

    private FrozenDictionary<Type, FrozenSet<string>> Members => members ??= NewMembers();

    private FrozenDictionary<Type, FrozenSet<string>> NewMembers()
    {
        var map = default( Dictionary<Type, HashSet<string>> );
        var groups = provider.ApiDescriptionGroups.Items;
        var comparer = StringComparer.OrdinalIgnoreCase;

        for ( var i = 0; i < groups.Count; i++ )
        {
            var group = groups[i];

            if ( !comparer.Equals( group.GroupName, groupName ) )
            {
                continue;
            }

            var descriptions = group.Items;

            for ( var j = 0; j < descriptions.Count; j++ )
            {
                var description = descriptions[j];
                var parameters = description.ParameterDescriptions;

                for ( var k = 0; k < parameters.Count; k++ )
                {
                    var parameter = parameters[k];

                    if ( parameter.Source == BindingSource.Body )
                    {
                        Collect( ref map, parameter.ModelMetadata );
                    }
                }

                var responseTypes = description.SupportedResponseTypes;

                for ( var k = 0; k < responseTypes.Count; k++ )
                {
                    Collect( ref map, responseTypes[k].ModelMetadata );
                }
            }
        }

        if ( map is null )
        {
            return FrozenDictionary<Type, FrozenSet<string>>.Empty;
        }

        return map.ToFrozenDictionary( entry => entry.Key, entry => entry.Value.ToFrozenSet( StringComparer.Ordinal ) );
    }

    // the metadata of a model forms a tree that mirrors the shape of the schema. a type is only visited once, which
    // also stops a model that references itself, directly or transitively, from recursing forever
    private static void Collect( ref Dictionary<Type, HashSet<string>>? map, ModelMetadata? metadata )
    {
        if ( metadata is null )
        {
            return;
        }

        if ( metadata.ElementMetadata is { } elementMetadata )
        {
            Collect( ref map, elementMetadata );
            return;
        }

        // only metadata described for an API version reports an authoritative member list. metadata that describes
        // a model type as declared reports every member it has, which says nothing about the described API. a type
        // described with no visible members is meaningfully different from a type that was never described
        if ( metadata.DescribedApiVersion is null )
        {
            return;
        }

        var properties = metadata.Properties;

        map ??= [];

        if ( !map.TryAdd( metadata.ModelType, [] ) )
        {
            return;
        }

        var names = map[metadata.ModelType];

        for ( var i = 0; i < properties.Count; i++ )
        {
            var property = properties[i];

            if ( property.PropertyName is { Length: > 0 } name )
            {
                names.Add( name );
            }

            Collect( ref map, property );
        }
    }
}