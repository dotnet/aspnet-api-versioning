// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData;

using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;

[DebuggerDisplay( "{Name,nq} ({ApiVersion,nq})" )]
internal sealed class ClassSignature : IEquatable<ClassSignature>
{
    private static readonly ConstructorInfo newOriginalType = typeof( OriginalTypeAttribute ).GetConstructors()[0];
    private int? hashCode;

    internal ClassSignature( string name, Type originalType, IEnumerable<ClassProperty> properties, ApiVersion apiVersion )
    {
        var attributeBuilders = new List<CustomAttributeBuilder>()
        {
            new( newOriginalType, new[] { originalType } ),
        };

        attributeBuilders.AddRange( originalType.DeclaredAttributes() );

        Name = name;
        Attributes = attributeBuilders.ToArray();
        Properties = properties.ToArray();
        ApiVersion = apiVersion;
    }

    internal ClassSignature( string name, IEnumerable<ClassProperty> properties, ApiVersion apiVersion )
    {
        Name = name;
        Attributes = Array.Empty<CustomAttributeBuilder>();
        Properties = properties.ToArray();
        ApiVersion = apiVersion;
    }

    internal string Name { get; }

    internal IReadOnlyList<CustomAttributeBuilder> Attributes { get; }

    internal ClassProperty[] Properties { get; }

    internal ApiVersion ApiVersion { get; }

    public override int GetHashCode() => hashCode ??= ComputeHashCode();

    public override bool Equals( object? obj ) => obj is ClassSignature s && Equals( s );

    public bool Equals( ClassSignature? other ) => other != null && GetHashCode() == other.GetHashCode();

    private int ComputeHashCode()
    {
        var count = Properties.Length;

        if ( count == 0 )
        {
            return 0;
        }

        var hash = default( HashCode );
        ref var property = ref Properties[0];

        hash.Add( property );

        for ( var i = 1; i < count; i++ )
        {
            property = ref Properties[i];
            hash.Add( property );
        }

        return hash.ToHashCode();
    }
}