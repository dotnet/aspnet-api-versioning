// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApiExplorer;

using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

/// <summary>
/// Represents model metadata that reports only the members visible in a single API version.
/// </summary>
/// <remarks>
/// The metadata of a model is shared by every API version because it is keyed by the model type, which cannot
/// express a subset of its own members. A new instance is created per API version rather than mutating the shared
/// instance, which every version of a cloned API description otherwise points at.
/// </remarks>
internal sealed class VersionedModelMetadata : DelegatingModelMetadata
{
    private readonly IAnnotation<MemberInfo, ApiVersionRange> annotations;
    private readonly ApiVersion apiVersion;
    private IReadOnlyDictionary<object, object>? additionalValues;
    private ModelPropertyCollection? properties;
    private ModelMetadata? elementMetadata;
    private bool elementMetadataResolved;

    internal VersionedModelMetadata(
        ModelMetadata inner,
        IAnnotation<MemberInfo, ApiVersionRange> annotations,
        ApiVersion apiVersion )
        : this( inner, NewIdentity( inner ), annotations, apiVersion ) { }

    private VersionedModelMetadata(
        ModelMetadata inner,
        ModelMetadataIdentity identity,
        IAnnotation<MemberInfo, ApiVersionRange> annotations,
        ApiVersion apiVersion )
        : base( inner, identity )
    {
        this.annotations = annotations;
        this.apiVersion = apiVersion;
    }

    /// <inheritdoc />
    public override IReadOnlyDictionary<object, object> AdditionalValues =>
        additionalValues ??= NewAdditionalValues();

    /// <inheritdoc />
    /// <remarks>Evaluated on demand so that a model which references itself, directly or transitively, doesn't
    /// recurse.</remarks>
    public override ModelPropertyCollection Properties => properties ??= NewProperties();

    /// <inheritdoc />
    public override ModelMetadata? ElementMetadata
    {
        get
        {
            if ( !elementMetadataResolved )
            {
                elementMetadataResolved = true;
                elementMetadata = Wrap( Inner.ElementMetadata );
            }

            return elementMetadata;
        }
    }

    // ModelMetadataIdentity is only reachable through a protected member, so an equivalent identity is rebuilt
    // from the public surface of the metadata being wrapped
    [UnconditionalSuppressMessage(
        "ILLink",
        "IL2075",
        Justification = "MVC does not currently support trimming or native AOT. https://aka.ms/aspnet/trimming" )]
    private static ModelMetadataIdentity NewIdentity( ModelMetadata metadata )
    {
        if ( metadata.MetadataKind == ModelMetadataKind.Property &&
             metadata.ContainerType is { } containerType &&
             metadata.PropertyName is { Length: > 0 } propertyName &&
             containerType.GetProperty( propertyName ) is { } propertyInfo )
        {
            return ModelMetadataIdentity.ForProperty( propertyInfo, metadata.ModelType, containerType );
        }

        return ModelMetadataIdentity.ForType( metadata.ModelType );
    }

    // the API version is recorded so that a consumer can tell metadata which describes a subset of a model from
    // metadata which describes the model as declared
    private Dictionary<object, object> NewAdditionalValues()
    {
        var values = new Dictionary<object, object>( Inner.AdditionalValues.Count + 1 );

        foreach ( var pair in Inner.AdditionalValues )
        {
            values[pair.Key] = pair.Value;
        }

        values[typeof( ApiVersion )] = apiVersion;

        return values;
    }

    [UnconditionalSuppressMessage(
        "ILLink",
        "IL2075",
        Justification = "MVC does not currently support trimming or native AOT. https://aka.ms/aspnet/trimming" )]
    private ModelPropertyCollection NewProperties()
    {
        var innerProperties = Inner.Properties;
        var members = new List<ModelMetadata>( innerProperties.Count );

        for ( var i = 0; i < innerProperties.Count; i++ )
        {
            var property = innerProperties[i];

            // the declaring member is resolved from the container rather than the metadata identity, which is not
            // accessible outside the assembly that declares it
            if ( property.ContainerType is { } containerType &&
                 property.PropertyName is { Length: > 0 } propertyName &&
                 containerType.GetProperty( propertyName ) is { } propertyInfo &&
                 !annotations.IsVisible( propertyInfo, apiVersion ) )
            {
                continue;
            }

            members.Add( Wrap( property )! );
        }

        return new( members );
    }

    [return: NotNullIfNotNull( nameof( metadata ) )]
    private ModelMetadata? Wrap( ModelMetadata? metadata ) =>
        metadata switch
        {
            null => default,

            // a model already described for an API version, such as one reported by another provider, is left as
            // it is. wrapping it a second time would filter members that were already filtered
            { } described when described.DescribedApiVersion is not null => described,

            // a type with no members of its own has nothing to filter. leaving it alone keeps it out of the set
            // of types that report an authoritative member list
            { Properties.Count: 0, ElementMetadata: null } simple => simple,

            _ => new VersionedModelMetadata( metadata, annotations, apiVersion ),
        };
}