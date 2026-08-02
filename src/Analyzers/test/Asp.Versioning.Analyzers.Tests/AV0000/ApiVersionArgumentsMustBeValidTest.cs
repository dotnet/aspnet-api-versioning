// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Analyzers.AV0000;

public class ApiVersionArgumentsMustBeValidTest
{
    private const string AV0003 = nameof( AV0003 );
    private const string AV0004 = nameof( AV0004 );
    private const string AV0005 = nameof( AV0005 );
    private const string AV0006 = nameof( AV0006 );
    private const string AV0007 = nameof( AV0007 );
    private const string AV0008 = nameof( AV0008 );

    [Theory]
    [InlineData( "[ApiVersion( 1.0 )]" )]
    [InlineData( "[ApiVersion( 0.0 )]" )]
    [InlineData( "[ApiVersion( 1 )]" )]
    [InlineData( "[ApiVersion( 2.0, \"beta\" )]" )]
    [InlineData( "[ApiVersion( 2016, 1, 1 )]" )]
    [InlineData( "[ApiVersion( 2016, 2, 29 )]" )]
    [InlineData( "[ApiVersion( 2016, 12, 31, \"alpha.1\" )]" )]
    [InlineData( "[ApiVersion( \"1.0\" )]" )]
    [InlineData( "[AdvertiseApiVersions( 1.0, 2.0, 3.0 )]" )]
    [InlineData( "[AdvertiseApiVersions( 2016, 2, 29 )]" )]
    public async Task analyzer_should_not_report_valid_arguments( string attribute )
    {
        // arrange
        var source = Attributed( attribute );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Theory]
    [InlineData( "[ApiVersion( -1.0 )]" )]
    [InlineData( "[ApiVersion( -0.1 )]" )]
    [InlineData( "[ApiVersion( -1 )]" )]
    [InlineData( "[ApiVersion( -1.0, \"beta\" )]" )]
    [InlineData( "[AdvertiseApiVersions( -1.0 )]" )]
    [InlineData( "[AdvertiseApiVersions( 1.0, -2.0 )]" )]
    [InlineData( "[AdvertiseApiVersions( 1.0, new[] { -2.0 } )]" )]
    public async Task analyzer_should_report_negative_version( string attribute )
    {
        // arrange
        var source = Attributed( attribute );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0004 );
    }

    [Theory]
    [InlineData( "[ApiVersion( 0, 1, 1 )]", AV0005 )]
    [InlineData( "[ApiVersion( -1, 1, 1 )]", AV0005 )]
    [InlineData( "[ApiVersion( 10000, 1, 1 )]", AV0005 )]
    [InlineData( "[ApiVersion( 2016, 0, 1 )]", AV0006 )]
    [InlineData( "[ApiVersion( 2016, 13, 1 )]", AV0006 )]
    [InlineData( "[ApiVersion( 2016, 1, 0 )]", AV0007 )]
    [InlineData( "[ApiVersion( 2016, 1, 32 )]", AV0007 )]
    public async Task analyzer_should_report_invalid_date_component( string attribute, string expected )
    {
        // arrange
        var source = Attributed( attribute );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( expected );
    }

    [Theory]
    [InlineData( "[ApiVersion( 2013, 2, 29 )]" )]
    [InlineData( "[ApiVersion( 2016, 4, 31 )]" )]
    [InlineData( "[ApiVersion( 2016, 2, 30 )]" )]
    [InlineData( "[AdvertiseApiVersions( 2013, 2, 29 )]" )]
    public async Task analyzer_should_report_date_that_does_not_exist( string attribute )
    {
        // arrange
        // each component is individually in range, so only the composed date is reported
        var source = Attributed( attribute );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0008 );
    }

    [Fact]
    public async Task analyzer_should_report_date_across_every_component()
    {
        // arrange
        var source = Attributed( "[ApiVersion( 2013, 2, 29 )]" );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        var span = diagnostics.Should().ContainSingle().Subject.Location.SourceSpan;

        source.Substring( span.Start, span.Length ).Should().Be( "2013, 2, 29" );
    }

    [Fact]
    public async Task analyzer_should_not_report_date_when_a_component_is_invalid()
    {
        // arrange
        // the composed date cannot be evaluated until every component is in range, so each
        // component is reported on its own and the date itself is not
        var source = Attributed( "[ApiVersion( 0, 13, 32 )]" );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Select( diagnostic => diagnostic.Id ).Should().BeEquivalentTo( [AV0005, AV0006, AV0007] );
    }

    [Theory]
    [InlineData( "[ApiVersion( 1.0, \"1bad\" )]" )]
    [InlineData( "[ApiVersion( 1.0, \"a-b\" )]" )]
    [InlineData( "[ApiVersion( 1.0, \"a b\" )]" )]
    [InlineData( "[ApiVersion( 2016, 1, 1, \"a-b\" )]" )]
    [InlineData( "[AdvertiseApiVersions( 1.0, \"a-b\" )]" )]
    public async Task analyzer_should_report_invalid_status( string attribute )
    {
        // arrange
        var source = Attributed( attribute );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0003 );
    }

    [Fact]
    public async Task analyzer_should_report_map_to_api_version_arguments()
    {
        // arrange
        var source = """
            using Asp.Versioning;

            public class Controller
            {
                [MapToApiVersion( -1.0 )]
                public void Get() { }

                [MapToApiVersion( 2013, 2, 29 )]
                public void Put() { }

                [MapToApiVersion( 1.0, "a-b" )]
                public void Post() { }
            }
            """;

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Select( diagnostic => diagnostic.Id ).Should().BeEquivalentTo( [AV0004, AV0008, AV0003] );
    }

    [Theory]
    [InlineData( "builder.HasApiVersion( -1.0 )", AV0004 )]
    [InlineData( "builder.HasApiVersion( 1, -1 )", AV0004 )]
    [InlineData( "builder.HasApiVersion( 2013, 2, 29 )", AV0008 )]
    [InlineData( "builder.HasDeprecatedApiVersion( 2016, 13, 1 )", AV0006 )]
    [InlineData( "builder.AdvertisesApiVersion( 1.0, \"a b\" )", AV0003 )]
    [InlineData( "builder.AdvertisesDeprecatedApiVersion( -2.0 )", AV0004 )]
    [InlineData( "builder.MapToApiVersion( 1.0, \"a-b\" )", AV0003 )]
    [InlineData( "builder.MapToApiVersion( 0, 1, 1 )", AV0005 )]
    public async Task analyzer_should_report_convention_builder_arguments( string statement, string expected )
    {
        // arrange
        var source = Configured( statement );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( expected );
    }

    [Fact]
    public async Task analyzer_should_report_convention_builder_arguments_in_static_form()
    {
        // arrange
        // an extension member is declared in a synthetic nested type, so the receiver is only a
        // parameter when the method is called in its unreduced form
        var source = Configured( "ApiVersionConventionBuilderExtensions.HasApiVersion( builder, -1.0 )" );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0004 );
    }

    [Theory]
    [InlineData( "builder.HasApiVersion( 1.0 )" )]
    [InlineData( "builder.HasApiVersion( 1, 0 )" )]
    [InlineData( "builder.HasApiVersion( 2016, 2, 29 )" )]
    [InlineData( "builder.MapToApiVersion( 1.0, \"beta\" )" )]
    public async Task analyzer_should_not_report_valid_convention_builder_arguments( string statement )
    {
        // arrange
        var source = Configured( statement );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_not_report_arguments_known_only_at_run_time()
    {
        // arrange
        var source = """
            using Asp.Versioning;
            using Asp.Versioning.Conventions;

            public class Sample
            {
                public void Configure( IMapToApiVersionConventionBuilder builder, double version, string status )
                {
                    builder.HasApiVersion( version, status );
                }
            }
            """;

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_not_report_unrelated_api()
    {
        // arrange
        // the parameter names match, but the declaring type is not part of the versioning surface
        var source = """
            public static class Unrelated
            {
                public static void Configure( double version, string status ) { }

                public static void Configure( int year, int month, int day ) { }

                public static void Run()
                {
                    Configure( -1.0, "a-b" );
                    Configure( 2013, 2, 29 );
                }
            }
            """;

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_not_report_string_version()
    {
        // arrange
        // a string version is the concern of AV0001, even though the parameter is also named version
        var source = Attributed( "[ApiVersion( \"neutral\" )]" );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( "AV0001" );
    }

    [Fact]
    public async Task analyzer_should_report_object_creation()
    {
        // arrange
        var source = """
            using Asp.Versioning;

            public class Sample
            {
                public void Create()
                {
                    var attribute = new ApiVersionAttribute( -1.0 );
                    ApiVersionAttribute other = new( 2013, 2, 29 );
                }
            }
            """;

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Select( diagnostic => diagnostic.Id ).Should().BeEquivalentTo( [AV0004, AV0008] );
    }

    private static string Attributed( string attribute ) =>
        $$"""
        using Asp.Versioning;

        {{attribute}}
        public class Controller
        {
        }
        """;

    private static string Configured( string statement ) =>
        $$"""
        using Asp.Versioning;
        using Asp.Versioning.Conventions;

        public class Sample
        {
            public void Configure( IMapToApiVersionConventionBuilder builder )
            {
                {{statement}};
            }
        }
        """;
}