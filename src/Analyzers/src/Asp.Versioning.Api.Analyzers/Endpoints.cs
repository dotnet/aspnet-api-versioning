// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Analyzers;

/// <remarks>
/// An endpoint is an action or a mapped route, along with every template it answers to. A controller
/// contributes the templates it declares to each of its actions, which is how the same action comes to
/// be registered more than once.
/// </remarks>
internal static class Endpoints
{
    private static readonly HashSet<string> MapMethods = new( StringComparer.Ordinal )
    {
        "MapGet", "MapPost", "MapPut", "MapDelete", "MapPatch", "MapMethods", "Map",
    };

    public static bool IsMapped( string methodName ) => MapMethods.Contains( methodName );

    public static IEnumerable<Endpoint> FromController( INamedTypeSymbol type )
    {
        var controllerTemplates = Routes.GetTemplates( type );
        var versionedByController = Symbols.HasAttribute( type, Symbols.ApiVersionAttribute );
        var neutralByController = Symbols.HasAttribute( type, Symbols.ApiVersionNeutralAttribute );

        foreach ( var member in type.GetMembers() )
        {
            if ( member is not IMethodSymbol action ||
                 action.MethodKind != MethodKind.Ordinary ||
                 action.DeclaredAccessibility != Accessibility.Public ||
                 action.IsStatic )
            {
                continue;
            }

            var actionTemplates = Routes.GetTemplates( action );
            var versioned = versionedByController || Symbols.HasAttribute( action, Symbols.ApiVersionAttribute );
            var neutral = neutralByController || Symbols.HasAttribute( action, Symbols.ApiVersionNeutralAttribute );

            yield return new(
                Combine( controllerTemplates, actionTemplates ),
                versioned,
                neutral,
                type.ContainingNamespace?.ToDisplayString() );
        }
    }

    public static bool TryResolveConstraintName( IEnumerable<string> names, out string constraintName )
    {
        constraintName = RouteTemplate.DefaultConstraintName;
        var configured = false;

        foreach ( var name in names )
        {
            // more than one name in a compilation cannot resolve to a single answer
            if ( configured && name != constraintName )
            {
                return false;
            }

            constraintName = name;
            configured = true;
        }

        return true;
    }

    private static IReadOnlyList<string> Combine(
        IReadOnlyList<string> controllerTemplates,
        IReadOnlyList<string> actionTemplates )
    {
        if ( controllerTemplates.Count == 0 && actionTemplates.Count == 0 )
        {
            // routed by convention rather than by template, which cannot carry a constraint
            return [string.Empty];
        }

        if ( controllerTemplates.Count == 0 )
        {
            return actionTemplates;
        }

        if ( actionTemplates.Count == 0 )
        {
            return controllerTemplates;
        }

        var combined = new List<string>( controllerTemplates.Count * actionTemplates.Count );

        foreach ( var controllerTemplate in controllerTemplates )
        {
            foreach ( var actionTemplate in actionTemplates )
            {
                combined.Add( controllerTemplate + "/" + actionTemplate );
            }
        }

        return combined;
    }
}