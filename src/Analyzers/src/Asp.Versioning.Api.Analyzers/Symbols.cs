// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Analyzers;

/// <summary>
/// The API surface the rules apply to is matched by name because it is not available to an analyzer, which the compiler
/// requires to target netstandard2.0.
/// </summary>
internal static class Symbols
{
    public const string ApiVersioningOptions = "Asp.Versioning.ApiVersioningOptions";
    public const string MvcApiVersioningOptions = "Asp.Versioning.MvcApiVersioningOptions";
    public const string ODataApiVersioningOptions = "Asp.Versioning.OData.ODataApiVersioningOptions";
    public const string ApiExplorerOptions = "Asp.Versioning.ApiExplorer.ApiExplorerOptions";
    public const string ODataApiExplorerOptions = "Asp.Versioning.ApiExplorer.ODataApiExplorerOptions";
    public const string ServiceCollectionExtensions =
        "Microsoft.Extensions.DependencyInjection.IServiceCollectionExtensions";
    public const string EndpointRouteBuilderExtensions =
        "Microsoft.AspNetCore.Builder.EndpointRouteBuilderExtensions";
    public const string WebApplication = "Microsoft.AspNetCore.Builder.WebApplication";
    public const string ControllerBase = "Microsoft.AspNetCore.Mvc.ControllerBase";
    public const string Controller = "Microsoft.AspNetCore.Mvc.Controller";
    public const string ODataController = "Microsoft.AspNetCore.OData.Routing.Controllers.ODataController";
    public const string RouteAttribute = "Microsoft.AspNetCore.Mvc.RouteAttribute";
    public const string HttpMethodAttributePrefix = "Microsoft.AspNetCore.Mvc.Http";
    public const string ApiVersionAttribute = "Asp.Versioning.ApiVersionAttribute";
    public const string ApiVersionNeutralAttribute = "Asp.Versioning.ApiVersionNeutralAttribute";

    // an extension member is declared in a synthetic, nested type that cannot be referred to by name, so the type
    // that declares the member is its containing type.
    public static INamedTypeSymbol? ResolveDeclaringType( IMethodSymbol method )
    {
        var type = method.ContainingType;

        return type is { ContainingType: { } declaringType } && !type.CanBeReferencedByName
             ? declaringType
             : type;
    }

    public static string? GetDeclaringType( SyntaxNodeAnalysisContext context, InvocationExpressionSyntax invocation ) =>
        context.SemanticModel.GetSymbolInfo( invocation, context.CancellationToken ).Symbol
        is IMethodSymbol method && ResolveDeclaringType( method ) is { } type
        ? type.ToDisplayString()
        : default;

    public static bool Inherits( INamedTypeSymbol type, string baseTypeName )
    {
        for ( var baseType = type.BaseType; baseType is not null; baseType = baseType.BaseType )
        {
            if ( baseType.ToDisplayString() == baseTypeName )
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether a type is, or derives from, a named type.
    /// </summary>
    /// <remarks>A member declared by a base type is reached through the type that derives from it, so the
    /// type a member appears to belong to is not always the type that declares it.</remarks>
    public static bool Declares( INamedTypeSymbol? type, string typeName )
    {
        for ( var declaringType = type; declaringType is not null; declaringType = declaringType.BaseType )
        {
            if ( declaringType.ToDisplayString() == typeName )
            {
                return true;
            }
        }

        return false;
    }

    public static bool HasAttribute( ISymbol symbol, string attributeName )
    {
        foreach ( var attribute in symbol.GetAttributes() )
        {
            if ( attribute.AttributeClass?.ToDisplayString() == attributeName )
            {
                return true;
            }
        }

        return false;
    }

    // a controller derived from Controller serves a user interface, which is never versioned, and one derived from
    // ODataController is routed by its registered components rather than by an attribute
    public static bool IsApiController( INamedTypeSymbol type ) =>
        type is { TypeKind: TypeKind.Class, IsAbstract: false } &&
        Inherits( type, ControllerBase ) &&
        !Inherits( type, Controller ) &&
        !Inherits( type, ODataController );

    /// <summary>
    /// Determines whether a type is available to a compilation.
    /// </summary>
    /// <remarks>A rule that depends on a specialized variant has nothing to match when the variant is
    /// not referenced, so the work it would do can be skipped entirely.</remarks>
    public static bool IsReferenced( Compilation compilation, string typeName ) =>
        compilation.GetTypeByMetadataName( typeName ) is not null;

    public static Location GetLocation( InvocationExpressionSyntax invocation ) =>
        invocation.Expression is MemberAccessExpressionSyntax access
        ? access.Name.GetLocation()
        : invocation.Expression.GetLocation();
}