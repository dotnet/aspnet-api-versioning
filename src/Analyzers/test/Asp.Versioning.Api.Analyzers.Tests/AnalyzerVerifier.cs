// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Analyzers;

internal static class AnalyzerVerifier
{
    private static readonly ImmutableArray<MetadataReference> References = CreateReferences();

    public static Task<ImmutableArray<Diagnostic>> AnalyzeAsync( params string[] sources ) =>
        AnalyzeAsync( OutputKind.DynamicallyLinkedLibrary, sources );

    // a rule can depend on whether the compilation produces an application, which only an entry point makes it
    public static async Task<ImmutableArray<Diagnostic>> AnalyzeAsync(
        OutputKind outputKind,
        params string[] sources )
    {
        var compilation = CSharpCompilation.Create(
            "Test",
            sources.Select( source => CSharpSyntaxTree.ParseText( source ) ),
            References,
            new CSharpCompilationOptions( outputKind ) );

        compilation.GetDiagnostics()
                   .Where( diagnostic => diagnostic.Severity == DiagnosticSeverity.Error )
                   .Should()
                   .BeEmpty( "the source under test must compile" );

        // every analyzer runs for every test so that no analyzer reports on another's concern
        var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(
            new DefaultApiVersionAnalyzer(),
            new MissingAddMvcAnalyzer(),
            new MissingApiBehaviorAnalyzer(),
            new SpecificApiVersionReaderAnalyzer(),
            new AssumeDefaultApiVersionAnalyzer(),
            new DefaultValueAnalyzer(),
            new AllEndpointsVersionNeutralAnalyzer(),
            new VersionedAndNeutralAnalyzer(),
            new ApiExplorerAnalyzer(),
            new MissingAddODataAnalyzer(),
            new IgnoredRouteComponentsAnalyzer(),
            new InheritedApiExplorerOptionAnalyzer(),
            new MissingDocumentInfoAnalyzer(),
            new UnusedGroupNameFormatAnalyzer(),
            new DescribeApiVersionsAnalyzer(),
            new PolicyEffectiveDateAnalyzer(),
            new VersionedOpenApiAnalyzer(),
            new MissingApiExplorerAnalyzer() );

        return await compilation.WithAnalyzers( analyzers )
                                .GetAnalyzerDiagnosticsAsync( TestContext.Current.CancellationToken )
                                .ConfigureAwait( false );
    }

    private static ImmutableArray<MetadataReference> CreateReferences()
    {
        // the trusted platform assemblies are the exact set the test host was loaded with, which
        // includes the runtime, the versioning libraries, and everything else in the output
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