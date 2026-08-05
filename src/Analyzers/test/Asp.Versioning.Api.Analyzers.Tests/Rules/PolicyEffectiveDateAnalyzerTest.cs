// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Analyzers.Rules;

public class PolicyEffectiveDateAnalyzerTest
{
    private const string AV0028 = nameof( AV0028 );

    [Fact]
    public async Task analyzer_should_report_a_sunset_before_its_deprecation()
    {
        // arrange
        var source = Configured( """
            options.Policies.Deprecate( 0.9 ).Effective( 2024, 6, 1 );
            options.Policies.Sunset( 0.9 ).Effective( 2024, 1, 1 );
            """ );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0028 );
    }

    [Fact]
    public async Task analyzer_should_not_report_a_sunset_after_its_deprecation()
    {
        // arrange
        // deprecation announces that an API is going away and sunset is when it does
        var source = Configured( """
            options.Policies.Deprecate( 0.9 ).Effective( 2024, 1, 1 );
            options.Policies.Sunset( 0.9 ).Effective( 2024, 6, 1 );
            """ );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_not_report_the_same_day()
    {
        // arrange
        var source = Configured( """
            options.Policies.Deprecate( 0.9 ).Effective( 2024, 1, 1 );
            options.Policies.Sunset( 0.9 ).Effective( 2024, 1, 1 );
            """ );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Theory]
    [InlineData( "0.9", "0.9" )]
    [InlineData( "\"Orders\"", "\"Orders\"" )]
    [InlineData( "\"Orders\", 1, 0", "\"Orders\", 1, 0" )]
    [InlineData( "new ApiVersion( 2, 0 )", "new ApiVersion( 2, 0 )" )]
    [InlineData( "1.0", "new ApiVersion( 1, 0 )" )]
    [InlineData( "\"Orders\", 1.0", "\"Orders\", new ApiVersion( 1 )" )]
    public async Task analyzer_should_report_policies_keyed_the_same_way( string deprecated, string sunset )
    {
        // arrange
        var source = Configured( $"""
            options.Policies.Deprecate( {deprecated} ).Effective( 2024, 6, 1 );
            options.Policies.Sunset( {sunset} ).Effective( 2024, 1, 1 );
            """ );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0028 );
    }

    [Theory]
    [InlineData( "\"Orders\"", "0.9" )]
    [InlineData( "0.9", "\"Orders\"" )]
    [InlineData( "\"Orders\", 0.9", "\"Orders\"" )]
    [InlineData( "\"Orders\", 0.9", "0.9" )]
    public async Task analyzer_should_report_policies_an_api_reaches_together( string deprecated, string sunset )
    {
        // arrange
        // a policy that leaves a part unstated is reached by every API that agrees with the rest
        var source = Configured( $"""
            options.Policies.Deprecate( {deprecated} ).Effective( 2024, 6, 1 );
            options.Policies.Sunset( {sunset} ).Effective( 2024, 1, 1 );
            """ );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0028 );
    }

    [Theory]
    [InlineData( "\"Orders\"", "\"People\"" )]
    [InlineData( "0.9", "1.0" )]
    [InlineData( "\"Orders\", 0.9", "\"Orders\", 1.0" )]
    [InlineData( "\"Orders\", 0.9", "\"People\", 0.9" )]
    [InlineData( "\"Orders\", 0.9", "1.0" )]
    public async Task analyzer_should_not_report_policies_no_api_reaches_together(
        string deprecated,
        string sunset )
    {
        // arrange
        var source = Configured( $"""
            options.Policies.Deprecate( {deprecated} ).Effective( 2024, 6, 1 );
            options.Policies.Sunset( {sunset} ).Effective( 2024, 1, 1 );
            """ );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Theory]
    [InlineData( "DateTimeOffset.Now" )]
    [InlineData( "DateTimeOffset.Now.AddDays( 60 )" )]
    [InlineData( "date" )]
    public async Task analyzer_should_not_report_a_date_from_somewhere_else( string date )
    {
        // arrange
        // what the date will be is not decided here
        var source = $$"""
            using System;
            using Asp.Versioning;

            public static class Startup
            {
                public static void Configure( ApiVersioningOptions options, DateTimeOffset date )
                {
                    options.Policies.Deprecate( 0.9 ).Effective( 2024, 6, 1 );
                    options.Policies.Sunset( 0.9 ).Effective( {{date}} );
                }
            }
            """;

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_report_a_date_built_from_its_parts()
    {
        // arrange
        var source = Configured( """
            options.Policies.Deprecate( 0.9 ).Effective( new DateTimeOffset( new DateTime( 2024, 6, 1 ) ) );
            options.Policies.Sunset( 0.9 ).Effective( new DateTimeOffset( new DateTime( 2024, 1, 1 ) ) );
            """ );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0028 );
    }

    [Theory]
    [InlineData( "\"\"" )]
    [InlineData( "default( string ), default( ApiVersion )" )]
    public async Task analyzer_should_not_report_a_policy_no_api_reaches( string key )
    {
        // arrange
        // stating neither a name nor a version reaches nothing at all rather than everything
        var source = Configured( $"""
            options.Policies.Deprecate( {key} ).Effective( 2024, 6, 1 );
            options.Policies.Sunset( 0.9 ).Effective( 2024, 1, 1 );
            """ );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_not_report_a_policy_without_a_date()
    {
        // arrange
        // a policy that never takes effect has nothing to compare
        var source = Configured( """
            options.Policies.Deprecate( 0.9 ).Effective( 2024, 6, 1 );
            options.Policies.Sunset( 0.9 ).Link( "policy.html" );
            """ );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_not_report_a_deprecation_alone()
    {
        // arrange
        var source = Configured( "options.Policies.Deprecate( 0.9 ).Effective( 2024, 6, 1 );" );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_report_a_sunset_once_for_any_number_of_deprecations()
    {
        // arrange
        var source = Configured( """
            options.Policies.Deprecate( "Orders" ).Effective( 2024, 6, 1 );
            options.Policies.Deprecate( 0.9 ).Effective( 2024, 7, 1 );
            options.Policies.Sunset( "Orders", 0.9 ).Effective( 2024, 1, 1 );
            """ );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0028 );
    }

    [Fact]
    public async Task analyzer_should_report_across_files()
    {
        // arrange
        var deprecation = Configured(
            "options.Policies.Deprecate( 0.9 ).Effective( 2024, 6, 1 );",
            "Deprecation" );
        var sunset = Configured(
            "options.Policies.Sunset( 0.9 ).Effective( 2024, 1, 1 );",
            "Sunset" );

        // act
        var diagnostics = await AnalyzeAsync( deprecation, sunset );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0028 );
    }

    [Fact]
    public async Task analyzer_should_report_at_the_sunset_date()
    {
        // arrange
        var source = Configured( """
            options.Policies.Deprecate( 0.9 ).Effective( 2024, 6, 1 ).Link( "policy.html" ).Title( "t" );
            options.Policies.Sunset( 0.9 ).Effective( 2024, 1, 1 ).Link( "policy.html" ).Title( "t" );
            """ );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        var diagnostic = diagnostics.Should().ContainSingle().Subject;
        var span = diagnostic.Location.SourceSpan;
        var line = diagnostic.Location.GetLineSpan().StartLinePosition.Line;

        source.Substring( span.Start, span.Length ).Should().Be( "Effective" );
        source.Split( '\n' )[line].Should().Contain( "Sunset" );
        diagnostic.Severity.Should().Be( DiagnosticSeverity.Warning );
    }

    // other rules can legitimately apply to the same configuration, so each test is scoped to its own
    private static async Task<IReadOnlyList<Diagnostic>> AnalyzeAsync( params string[] sources ) =>
        [.. ( await AnalyzerVerifier.AnalyzeAsync( sources ) ).Where( diagnostic => diagnostic.Id == AV0028 )];

    private static string Configured( string policies, string name = "Startup" ) =>
        $$"""
        using System;
        using Asp.Versioning;

        public static class {{name}}
        {
            public static void Configure( ApiVersioningOptions options )
            {
                {{policies}}
            }
        }
        """;
}