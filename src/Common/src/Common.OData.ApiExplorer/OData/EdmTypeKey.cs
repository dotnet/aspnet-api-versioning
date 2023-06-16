// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData;

using Microsoft.OData.Edm;

internal readonly struct EdmTypeKey : IEquatable<EdmTypeKey>
{
    private readonly int hashCode;

    public readonly string FullName;
    public readonly ApiVersion ApiVersion;

    internal EdmTypeKey( IEdmStructuredType type, ApiVersion apiVersion ) =>
        hashCode = HashCode.Combine( FullName = type.FullTypeName(), ApiVersion = apiVersion );

    internal EdmTypeKey( IEdmTypeReference type, ApiVersion apiVersion ) =>
        hashCode = HashCode.Combine( FullName = type.FullName(), ApiVersion = apiVersion );

    internal EdmTypeKey( string fullTypeName, ApiVersion apiVersion ) =>
        hashCode = HashCode.Combine( FullName = fullTypeName, ApiVersion = apiVersion );

    public static bool operator ==( EdmTypeKey obj, EdmTypeKey other ) => obj.Equals( other );

    public static bool operator !=( EdmTypeKey obj, EdmTypeKey other ) => !obj.Equals( other );

    public override int GetHashCode() => hashCode;

    public override bool Equals( object? obj ) => obj is EdmTypeKey other && Equals( other );

    public bool Equals( EdmTypeKey other ) => hashCode == other.hashCode;
}