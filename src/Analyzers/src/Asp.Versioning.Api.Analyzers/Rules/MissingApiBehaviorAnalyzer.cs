// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace Asp.Versioning.Analyzers;

using System.Collections.Immutable;
using static Descriptor;
using static Microsoft.CodeAnalysis.Diagnostics.GeneratedCodeAnalysisFlags;

/// <summary>
/// Represents an analyzer that reports a controller which has not opted into API behavior.
/// </summary>
/// <remarks>
/// API behavior can be applied to an assembly, either in code or generated from a build, in which case it covers every
/// controller and there is nothing left to report. It is otherwise applied per controller. A controller derived from
/// Controller is assumed to serve a user interface rather than an API, which is the ambiguity that applying the
/// attribute resolves.
/// </remarks>
[DiagnosticAnalyzer( LanguageNames.CSharp )]
public sealed class MissingApiBehaviorAnalyzer : DiagnosticAnalyzer
{
    private const string ApiControllerAttribute = "Microsoft.AspNetCore.Mvc.ApiControllerAttribute";
    private const string ControllerBase = "Microsoft.AspNetCore.Mvc.ControllerBase";
    private const string Controller = "Microsoft.AspNetCore.Mvc.Controller";

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create( AV0014_MissingApiBehavior );

    public override void Initialize( AnalysisContext context )
    {
        context.ConfigureGeneratedCodeAnalysis( Analyze | ReportDiagnostics );
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction( OnCompilationStart );
    }

    private static void OnCompilationStart( CompilationStartAnalysisContext context )
    {
        // without MVC there are no controllers to apply API behavior to
        if ( !Symbols.IsReferenced( context.Compilation, ControllerBase ) )
        {
            return;
        }

        if ( HasApiBehavior( context.Compilation.Assembly.GetAttributes() ) )
        {
            return;
        }

        context.RegisterSymbolAction( OnNamedType, SymbolKind.NamedType );
    }

    private static void OnNamedType( SymbolAnalysisContext context )
    {
        var type = (INamedTypeSymbol) context.Symbol;

        if ( type is not { TypeKind: TypeKind.Class, IsAbstract: false, ContainingType: null } ||
             !IsApiController( type ) ||
             HasApiBehavior( type ) )
        {
            return;
        }

        var location = type.Locations.FirstOrDefault( location => location.IsInSource );

        if ( location is not null )
        {
            context.ReportDiagnostic( Diagnostic.Create( AV0014_MissingApiBehavior, location ) );
        }
    }

    private static bool IsApiController( INamedTypeSymbol type )
    {
        for ( var baseType = type.BaseType; baseType is not null; baseType = baseType.BaseType )
        {
            switch ( baseType.ToDisplayString() )
            {
                case Controller:
                    return false;
                case ControllerBase:
                    return true;
            }
        }

        return false;
    }

    private static bool HasApiBehavior( INamedTypeSymbol type )
    {
        for ( var declaringType = type; declaringType is not null; declaringType = declaringType.BaseType )
        {
            if ( HasApiBehavior( declaringType.GetAttributes() ) )
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasApiBehavior( ImmutableArray<AttributeData> attributes )
    {
        foreach ( var attribute in attributes )
        {
            if ( attribute.AttributeClass?.ToDisplayString() == ApiControllerAttribute )
            {
                return true;
            }
        }

        return false;
    }
}