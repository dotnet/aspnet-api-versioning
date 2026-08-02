// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Analyzers;

using System.Collections.Immutable;

/// <remarks>
/// Binds the arguments of an attribute, invocation, or object creation back to the symbol each argument
/// is passed to, which is where the metadata that drives an analyzer is declared.
/// </remarks>
internal static class Arguments
{
    public static IParameterSymbol? ResolveParameter(
        ImmutableArray<IParameterSymbol> parameters,
        string? name,
        int index )
    {
        if ( name is not null )
        {
            foreach ( var parameter in parameters )
            {
                if ( parameter.Name == name )
                {
                    return parameter;
                }
            }

            return default;
        }

        if ( index < parameters.Length )
        {
            return parameters[index];
        }

        // beyond the declared parameters the argument can only belong to an expanded params array
        var last = parameters.Length - 1;

        return last >= 0 && parameters[last].IsParams ? parameters[last] : default;
    }

    public static ISymbol? ResolveMember( INamedTypeSymbol? type, string name )
    {
        for ( var declaringType = type; declaringType is not null; declaringType = declaringType.BaseType )
        {
            foreach ( var member in declaringType.GetMembers( name ) )
            {
                if ( member is IPropertySymbol or IFieldSymbol )
                {
                    return member;
                }
            }
        }

        return default;
    }

    /// <remarks>An extension member is declared in a synthetic, nested extension type, so the type that
    /// declares the member is its containing type. The synthetic type cannot be referred to by name,
    /// which identifies it without an API that only a newer compiler would provide.</remarks>
    public static INamedTypeSymbol? ResolveDeclaringType( IMethodSymbol method )
    {
        var type = method.ContainingType;

        return type is { ContainingType: { } declaringType } && !type.CanBeReferencedByName
             ? declaringType
             : type;
    }

    public static SeparatedSyntaxList<ExpressionSyntax>? GetArrayElements( ExpressionSyntax expression ) =>
        expression switch
        {
            ArrayCreationExpressionSyntax { Initializer: { } initializer } => initializer.Expressions,
            ImplicitArrayCreationExpressionSyntax array => array.Initializer.Expressions,
            _ => default( SeparatedSyntaxList<ExpressionSyntax>? ),
        };
}