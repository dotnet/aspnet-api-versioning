// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData;

using Microsoft.OData.Edm;

internal readonly struct EdmModelKey : IEquatable<EdmModelKey>
{
    private readonly int hashCode;

    public readonly IEdmModel EdmModel;
    public readonly ApiVersion ApiVersion;

    internal EdmModelKey( IEdmModel model, ApiVersion apiVersion ) =>
        hashCode = HashCode.Combine( ( EdmModel = model ).GetHashCode(), ApiVersion = apiVersion );

    public static bool operator ==( EdmModelKey obj, EdmModelKey other ) => obj.Equals( other );

    public static bool operator !=( EdmModelKey obj, EdmModelKey other ) => !obj.Equals( other );

    public override int GetHashCode() => hashCode;

    public override bool Equals( object? obj ) => obj is EdmModelKey other && Equals( other );

    public bool Equals( EdmModelKey other ) => hashCode == other.hashCode;
}