// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Analyzers;

/// <remarks>
/// A route template expresses a constraint as <c>{parameter:constraint}</c>, so the constraint is
/// present when its name appears after a colon within a parameter. A constraint may be parameterized,
/// as in <c>{version:apiVersion(1.0)}</c>, and a parameter may end with a default or be optional.
/// </remarks>
internal static class RouteTemplate
{
    public const string DefaultConstraintName = "apiVersion";

    public static bool HasConstraint( string template, string constraintName )
    {
        var start = -1;

        for ( var i = 0; i < template.Length; i++ )
        {
            switch ( template[i] )
            {
                case '{':
                    start = -1;
                    break;
                case ':':
                    start = i + 1;
                    break;
                case '}' or '(' or '=' or '?':
                    if ( Matches( template, start, i, constraintName ) )
                    {
                        return true;
                    }

                    start = -1;
                    break;
            }
        }

        return false;
    }

    private static bool Matches( string template, int start, int end, string constraintName )
    {
        if ( start < 0 || end - start != constraintName.Length )
        {
            return false;
        }

        for ( var i = 0; i < constraintName.Length; i++ )
        {
            if ( template[start + i] != constraintName[i] )
            {
                return false;
            }
        }

        return true;
    }
}