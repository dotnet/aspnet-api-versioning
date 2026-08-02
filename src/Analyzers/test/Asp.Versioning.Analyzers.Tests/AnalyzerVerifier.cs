// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Analyzers;

internal static class AnalyzerVerifier
{
    private static readonly ImmutableArray<MetadataReference> References = CreateReferences();

    public static async Task<ImmutableArray<Diagnostic>> AnalyzeAsync( string source, params MetadataReference[] references )
    {
        var compilation = Compile( "Test", source, references );

        compilation.GetDiagnostics()
                   .Where( diagnostic => diagnostic.Severity == DiagnosticSeverity.Error )
                   .Should()
                   .BeEmpty( "the source under test must compile" );

        // every analyzer runs for every test so that no analyzer reports on another's syntax
        var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(
            new ApiVersionStringSyntaxMustBeValid(),
            new ApiVersionRangeStringSyntaxMustBeValid(),
            new ApiVersionFormatStringSyntaxMustBeValid(),
            new ApiVersionArgumentsMustBeValid() );

        return await compilation.WithAnalyzers( analyzers )
                                .GetAnalyzerDiagnosticsAsync( TestContext.Current.CancellationToken )
                                .ConfigureAwait( false );
    }

    public static MetadataReference EmitAssembly( string assemblyName, string source )
    {
        var stream = new MemoryStream();
        var result = Compile( assemblyName, source, [] ).Emit( stream );

        result.Success.Should().BeTrue( "the referenced assembly must compile" );
        stream.Position = 0;

        return MetadataReference.CreateFromStream( stream );
    }

    public static string Literal( string value ) => SymbolDisplay.FormatLiteral( value, quote: true );

    private static CSharpCompilation Compile( string assemblyName, string source, MetadataReference[] references ) =>
        CSharpCompilation.Create(
            assemblyName,
            [CSharpSyntaxTree.ParseText( source )],
            [.. References, .. references],
            new CSharpCompilationOptions( OutputKind.DynamicallyLinkedLibrary ) );

    private static ImmutableArray<MetadataReference> CreateReferences()
    {
        // the trusted platform assemblies are the exact set the test host was loaded with, which
        // includes the runtime, Asp.Versioning.Abstractions, and everything else in the output
        var assemblies = (string) AppContext.GetData( "TRUSTED_PLATFORM_ASSEMBLIES" );

        return
        [
            .. assemblies
                .Split( Path.PathSeparator )
                .Where( path => path.EndsWith( ".dll", StringComparison.OrdinalIgnoreCase ) )
                .GroupBy( Path.GetFileName )
                .Select( duplicates => MetadataReference.CreateFromFile( duplicates.First() ) )
        ];
    }
}