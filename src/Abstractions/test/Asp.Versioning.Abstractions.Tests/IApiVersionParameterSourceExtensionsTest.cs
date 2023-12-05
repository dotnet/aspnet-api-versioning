// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using static Asp.Versioning.ApiVersionParameterLocation;

public class IApiVersionParameterSourceExtensionsTest
{
    [Theory]
    [InlineData( Path, false )]
    [InlineData( Query, true )]
    public void versions_by_query_string_should_return_expected_result( ApiVersionParameterLocation location, bool expected )
    {
        // arrange
        var source = new Mock<IApiVersionParameterSource>();

        source.Setup( s => s.AddParameters( It.IsAny<IApiVersionParameterDescriptionContext>() ) )
              .Callback( ( IApiVersionParameterDescriptionContext context ) => context.AddParameter( "", location ) );

        // act
        var result = source.Object.VersionsByQueryString();

        // assert
        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( Query, false )]
    [InlineData( Header, true )]
    public void versions_by_header_should_return_expected_result( ApiVersionParameterLocation location, bool expected )
    {
        // arrange
        var source = new Mock<IApiVersionParameterSource>();

        source.Setup( s => s.AddParameters( It.IsAny<IApiVersionParameterDescriptionContext>() ) )
              .Callback( ( IApiVersionParameterDescriptionContext context ) => context.AddParameter( "", location ) );

        // act
        var result = source.Object.VersionsByHeader();

        // assert
        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( Query, false )]
    [InlineData( MediaTypeParameter, true )]
    public void versions_by_media_type_should_return_expected_result( ApiVersionParameterLocation location, bool expected )
    {
        // arrange
        var source = new Mock<IApiVersionParameterSource>();

        source.Setup( s => s.AddParameters( It.IsAny<IApiVersionParameterDescriptionContext>() ) )
              .Callback( ( IApiVersionParameterDescriptionContext context ) => context.AddParameter( "", location ) );

        // act
        var result = source.Object.VersionsByMediaType();

        // assert
        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( Query, false )]
    [InlineData( Path, true )]
    public void versions_by_url_should_return_expected_result( ApiVersionParameterLocation location, bool expected )
    {
        // arrange
        var source = new Mock<IApiVersionParameterSource>();

        source.Setup( s => s.AddParameters( It.IsAny<IApiVersionParameterDescriptionContext>() ) )
              .Callback( ( IApiVersionParameterDescriptionContext context ) =>
              {
                  context.AddParameter( "", location );
                  context.AddParameter( "", Header );
              } );

        // act
        var result = source.Object.VersionsByUrl();

        // assert
        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( Query, false )]
    [InlineData( Path, true )]
    public void versions_by_url_should_return_expected_result_with_multiple_locations( ApiVersionParameterLocation location, bool expected )
    {
        // arrange
        var source = new Mock<IApiVersionParameterSource>();

        source.Setup( s => s.AddParameters( It.IsAny<IApiVersionParameterDescriptionContext>() ) )
              .Callback( ( IApiVersionParameterDescriptionContext context ) => context.AddParameter( "", location ) );

        // act
        var result = source.Object.VersionsByUrl( allowMultipleLocations: false );

        // assert
        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( Query, "api-version" )]
    [InlineData( Header, "" )]
    public void get_parameter_name_should_return_expected_value( ApiVersionParameterLocation location, string expected )
    {
        // arrange
        var source = new Mock<IApiVersionParameterSource>();

        source.Setup( s => s.AddParameters( It.IsAny<IApiVersionParameterDescriptionContext>() ) )
              .Callback( ( IApiVersionParameterDescriptionContext context ) => context.AddParameter( "api-version", Query ) );

        // act
        var name = source.Object.GetParameterName( location );

        // assert
        name.Should().Be( expected );
    }

    [Theory]
    [InlineData( Path, "" )]
    [InlineData( Query, "api-version" )]
    public void get_parameter_name_should_return_first_matching_name( ApiVersionParameterLocation location, string expected )
    {
        // arrange
        var source = new Mock<IApiVersionParameterSource>();

        source.Setup( s => s.AddParameters( It.IsAny<IApiVersionParameterDescriptionContext>() ) )
              .Callback( ( IApiVersionParameterDescriptionContext context ) =>
              {
                  context.AddParameter( "", Path );
                  context.AddParameter( "api-version", Query );
                  context.AddParameter( "x-ms-version", Header );
              } );

        // act
        var name = source.Object.GetParameterName( location );

        // assert
        name.Should().Be( expected );
    }

    [Fact]
    public void get_parameter_names_should_return_matching_names()
    {
        // arrange
        var source = new Mock<IApiVersionParameterSource>();
        var expected = new[] { "api-version", "ver" };

        source.Setup( s => s.AddParameters( It.IsAny<IApiVersionParameterDescriptionContext>() ) )
              .Callback( ( IApiVersionParameterDescriptionContext context ) =>
              {
                  context.AddParameter( "api-version", Query );
                  context.AddParameter( "x-ms-version", Header );
                  context.AddParameter( "ver", Query );
                  context.AddParameter( "", Path );
              } );

        // act
        var names = source.Object.GetParameterNames( Query );

        // assert
        names.Should().BeEquivalentTo( expected );
    }
}