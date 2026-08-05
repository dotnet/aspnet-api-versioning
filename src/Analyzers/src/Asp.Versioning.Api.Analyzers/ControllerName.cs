// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Analyzers;

/// <remarks>
/// This is a compile-time port of Asp.Versioning.Conventions.ControllerNameConvention. Controllers are
/// collated by a logical name rather than by route template, because templates that differ across
/// versions can still describe the same API. Any change to the convention must be reflected here.
/// </remarks>
internal static class ControllerName
{
    private const string Controller = nameof( Controller );
    private const string ControllerNameAttribute = "Asp.Versioning.ControllerNameAttribute";

    /// <summary>
    /// Gets the logical name a controller is collated under.
    /// </summary>
    /// <remarks>An explicitly declared name is taken as given. Otherwise the Controller suffix is
    /// removed and trailing numbers are trimmed, so that Example, ExampleController, and
    /// Example2Controller all collate together.</remarks>
    public static bool TryResolve( INamedTypeSymbol type, out string name )
    {
        foreach ( var attribute in type.GetAttributes() )
        {
            if ( attribute.AttributeClass?.ToDisplayString() != ControllerNameAttribute )
            {
                continue;
            }

            if ( attribute.ConstructorArguments.Length > 0 &&
                 attribute.ConstructorArguments[0].Value is string declared )
            {
                name = declared;
                return true;
            }

            // a name that cannot be read leaves the collation unknown
            name = string.Empty;
            return false;
        }

        name = TrimTrailingNumbers( RemoveSuffix( type.Name ) );
        return true;
    }

    private static string RemoveSuffix( string name ) =>
        name.Length > Controller.Length && name.EndsWith( Controller, StringComparison.Ordinal )
        ? name.Substring( 0, name.Length - Controller.Length )
        : name;

    private static string TrimTrailingNumbers( string name )
    {
        if ( name.Length == 0 )
        {
            return string.Empty;
        }

        var last = name.Length - 1;

        for ( var i = last; i >= 0; i-- )
        {
            if ( !char.IsNumber( name[i] ) )
            {
                return i < last ? name.Substring( 0, i + 1 ) : name;
            }
        }

        return name;
    }
}