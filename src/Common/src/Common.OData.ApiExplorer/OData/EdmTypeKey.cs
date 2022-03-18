// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData;

using Microsoft.OData.Edm;

internal readonly struct EdmTypeKey : IEquatable<EdmTypeKey>
{
    private readonly int hashCode;

    internal EdmTypeKey( IEdmStructuredType type, ApiVersion apiVersion ) =>
        hashCode = HashCode.Combine( type.FullTypeName(), apiVersion );

    internal EdmTypeKey( IEdmTypeReference type, ApiVersion apiVersion ) =>
        hashCode = HashCode.Combine( type.FullName(), apiVersion );

    internal EdmTypeKey( string fullTypeName, ApiVersion apiVersion ) =>
        hashCode = HashCode.Combine( fullTypeName, apiVersion );

    public static bool operator ==( EdmTypeKey obj, EdmTypeKey other ) => obj.Equals( other );

    public static bool operator !=( EdmTypeKey obj, EdmTypeKey other ) => !obj.Equals( other );

    public override int GetHashCode() => hashCode;

    public override bool Equals( object? obj ) => obj is EdmTypeKey other && Equals( other );

    public bool Equals( EdmTypeKey other ) => hashCode == other.hashCode;
}