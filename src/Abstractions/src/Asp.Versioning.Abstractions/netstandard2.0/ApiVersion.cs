// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

/// <content>
/// Contains additional implementation specific to .NET Standard 2.0.
/// </content>
public partial class ApiVersion
{
    /// <summary>
    /// Gets a value indicating whether the specified status is valid.
    /// </summary>
    /// <param name="status">The status to evaluate.</param>
    /// <returns>True if the status is valid; otherwise, false.</returns>
    /// <remarks>The status must be alphabetic or alphanumeric, start with a letter, and contain no spaces.</remarks>
    public static bool IsValidStatus( string? status ) => status is null || IsValidStatus( status.AsSpan() );

    /// <summary>
    /// Gets a value indicating whether the specified status is valid.
    /// </summary>
    /// <param name="status">The status to evaluate.</param>
    /// <returns>True if the status is valid; otherwise, false.</returns>
    /// <remarks>The status must be alphabetic or alphanumeric, start with a letter, and contain no spaces.</remarks>
    public static bool IsValidStatus( ReadOnlySpan<char> status )
    {
        if ( status.IsEmpty )
        {
            return true;
        }

        ref readonly var ch = ref status[0];

        if ( !char.IsLetter( ch ) )
        {
            return false;
        }

        for ( var i = 1; i < status.Length; i++ )
        {
            ch = ref status[i];

            if ( !char.IsLetterOrDigit( ch ) && ch != '.' )
            {
                return false;
            }
        }

        return true;
    }
}