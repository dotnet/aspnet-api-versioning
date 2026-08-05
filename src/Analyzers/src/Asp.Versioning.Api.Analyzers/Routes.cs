// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Analyzers;

/// <remarks>
/// An endpoint is reached through a chain of builders. A group contributes a prefix to the routes
/// mapped onto it, and other calls in the chain pass the builder along unchanged. Following that chain
/// is how a route template and anything applied to it along the way are recovered.
/// </remarks>
internal static class Routes
{
    private const string MapGroup = nameof( MapGroup );

    public static IReadOnlyList<string> GetTemplates( ISymbol symbol )
    {
        List<string>? templates = default;

        foreach ( var attribute in symbol.GetAttributes() )
        {
            if ( attribute.AttributeClass is not { } type || attribute.ConstructorArguments.Length == 0 )
            {
                continue;
            }

            var name = type.ToDisplayString();
            var routed = name == Symbols.RouteAttribute ||
                         ( name.StartsWith( Symbols.HttpMethodAttributePrefix, StringComparison.Ordinal ) &&
                           name.EndsWith( "Attribute", StringComparison.Ordinal ) );

            if ( routed && attribute.ConstructorArguments[0].Value is string template )
            {
                ( templates ??= [] ).Add( template );
            }
        }

        return (IReadOnlyList<string>?) templates ?? [];
    }

    public static string? GetArgument(
        SyntaxNodeAnalysisContext context,
        InvocationExpressionSyntax invocation,
        IMethodSymbol method,
        string parameterName )
    {
        var arguments = invocation.ArgumentList.Arguments;

        for ( var i = 0; i < arguments.Count; i++ )
        {
            var argument = arguments[i];
            var name = argument.NameColon?.Name.Identifier.ValueText
                       ?? ( i < method.Parameters.Length ? method.Parameters[i].Name : default );

            if ( name != parameterName )
            {
                continue;
            }

            return context.SemanticModel.GetConstantValue( argument.Expression, context.CancellationToken )
                   is { HasValue: true, Value: string value }
                 ? value
                 : default;
        }

        return default;
    }

    /// <summary>
    /// Follows the chain an endpoint was built from, gathering the prefixes applied to it.
    /// </summary>
    /// <remarks>The chain is complete when it reaches the application itself. A chain that ends at a
    /// parameter, field, or property may be missing a prefix, so the template it produces can only be
    /// trusted when what was resolved already answers the question being asked.</remarks>
    public static string ResolveChain(
        SyntaxNodeAnalysisContext context,
        ExpressionSyntax? expression,
        ISet<string> applied,
        out bool complete )
    {
        var prefixes = string.Empty;

        for ( var node = expression; node is not null; )
        {
            switch ( node )
            {
                case InvocationExpressionSyntax invocation:
                    if ( context.SemanticModel.GetSymbolInfo( invocation, context.CancellationToken ).Symbol
                         is IMethodSymbol method )
                    {
                        applied.Add( method.Name );

                        if ( method.Name == MapGroup &&
                             Symbols.ResolveDeclaringType( method )?.ToDisplayString() ==
                             Symbols.EndpointRouteBuilderExtensions )
                        {
                            if ( GetArgument( context, invocation, method, "prefix" ) is not { } prefix )
                            {
                                complete = false;
                                return prefixes;
                            }

                            prefixes = prefix + "/" + prefixes;
                        }
                    }

                    node = Receiver( invocation );
                    break;

                case IdentifierNameSyntax or MemberAccessExpressionSyntax:
                    var symbol = context.SemanticModel.GetSymbolInfo( node, context.CancellationToken ).Symbol;

                    if ( symbol is ILocalSymbol local && GetInitializer( local ) is { } initializer )
                    {
                        node = initializer;
                        break;
                    }

                    // the application itself is the origin, so nothing further can prefix a route
                    complete = TypeOf( context, node ) == Symbols.WebApplication;
                    return prefixes;

                default:
                    complete = false;
                    return prefixes;
            }
        }

        complete = false;
        return prefixes;
    }

    /// <summary>
    /// Collects the calls chained onto an endpoint after it was mapped.
    /// </summary>
    public static void CollectChainedCalls( InvocationExpressionSyntax invocation, ISet<string> applied )
    {
        for ( SyntaxNode? node = invocation; node is not null; )
        {
            if ( node.Parent is not MemberAccessExpressionSyntax access ||
                 access.Parent is not InvocationExpressionSyntax chained )
            {
                return;
            }

            applied.Add( access.Name.Identifier.ValueText );
            node = chained;
        }
    }

    public static ExpressionSyntax? Receiver( InvocationExpressionSyntax invocation ) =>
        invocation.Expression is MemberAccessExpressionSyntax access ? access.Expression : default;

    private static ExpressionSyntax? GetInitializer( ILocalSymbol local )
    {
        foreach ( var reference in local.DeclaringSyntaxReferences )
        {
            if ( reference.GetSyntax() is VariableDeclaratorSyntax { Initializer.Value: { } value } )
            {
                return value;
            }
        }

        return default;
    }

    private static string? TypeOf( SyntaxNodeAnalysisContext context, SyntaxNode node ) =>
        context.SemanticModel.GetTypeInfo( (ExpressionSyntax) node, context.CancellationToken )
               .Type?.ToDisplayString();
}