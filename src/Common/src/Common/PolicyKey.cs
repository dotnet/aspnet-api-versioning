// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using System.Diagnostics;

internal readonly struct PolicyKey : IEquatable<PolicyKey>
{
    private readonly string? name;
    private readonly ApiVersion? version;

    public PolicyKey( string? name, ApiVersion? version )
    {
        this.name = string.IsNullOrEmpty( name ) ? default : name;
        this.version = version;

        Debug.Assert( name != null || version != null, $"'{nameof( name )}' and '{nameof( version )}' should not both be null." );
    }

    public bool Equals( PolicyKey other ) => GetHashCode() == other.GetHashCode();

    public override bool Equals( [NotNullWhen( true )] object? obj ) => obj is PolicyKey other && Equals( other );

    public override int GetHashCode()
    {
        var hashCode = default( HashCode );

        if ( name != null )
        {
            hashCode.Add( name, StringComparer.OrdinalIgnoreCase );
        }

        if ( version != null )
        {
            hashCode.Add( version );
        }

        return hashCode.ToHashCode();
    }
}