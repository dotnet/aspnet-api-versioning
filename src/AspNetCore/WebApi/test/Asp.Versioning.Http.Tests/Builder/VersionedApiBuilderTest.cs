// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Builder;

using Asp.Versioning.Conventions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;

public class VersionedApiBuilderTest
{
    [Fact]
    public void has_mapping_should_invoke_callback()
    {
        // arrange
        var routeBuilder = Mock.Of<IEndpointRouteBuilder>();
        var apiBuilder = new VersionedApiBuilder( routeBuilder );
        var map = Mock.Of<Action<VersionedEndpointBuilder>>();

        // act
        apiBuilder.HasMapping( map );

        // assert
        Mock.Get( map ).Verify( f => f( It.IsAny<VersionedEndpointBuilder>() ) );
    }

    [Fact]
    public void build_should_return_version_neutral_api_version_model()
    {
        // arrange
        var routeBuilder = Mock.Of<IEndpointRouteBuilder>();
        var apiBuilder = new VersionedApiBuilder( routeBuilder );

        apiBuilder.IsApiVersionNeutral();

        // act
        var model = apiBuilder.Build();

        // assert
        model.Should().BeSameAs( ApiVersionModel.Neutral );
    }

    [Fact]
    public void build_should_ignore_explicit_mapping_with_version_neutral()
    {
        // arrange
        var routeBuilder = Mock.Of<IEndpointRouteBuilder>();
        var apiBuilder = new VersionedApiBuilder( routeBuilder );

        apiBuilder.HasApiVersion( 2.0 );
        apiBuilder.IsApiVersionNeutral();

        // act
        var model = apiBuilder.Build();

        // assert
        model.DeclaredApiVersions.Should().BeEmpty();
    }

    [Fact]
    public void build_should_construct_api_version_model()
    {
        // arrange
        var routeBuilder = Mock.Of<IEndpointRouteBuilder>();
        var apiBuilder = new VersionedApiBuilder( routeBuilder );

        apiBuilder.HasApiVersion( 1.0 )
                  .HasDeprecatedApiVersion( 0.9 )
                  .AdvertisesApiVersion( 2.0 )
                  .AdvertisesDeprecatedApiVersion( 2.0, "Beta" );

        // act
        var model = apiBuilder.Build();

        // assert
        model.Should().BeEquivalentTo(
            new ApiVersionModel(
                declaredVersions: new ApiVersion[] { new( 0.9 ), new( 1.0 ) },
                supportedVersions: new ApiVersion[] { new( 1.0 ), new( 2.0 ) },
                deprecatedVersions: new ApiVersion[] { new( 0.9 ), new( 2.0, "Beta" ) },
                advertisedVersions: new ApiVersion[] { new( 2.0 ) },
                deprecatedAdvertisedVersions: new ApiVersion[] { new( 2.0, "Beta" ) } ) );
    }

    [Fact]
    public void build_should_construct_api_version_model_from_default_version()
    {
        // arrange
        var routeBuilder = new Mock<IEndpointRouteBuilder>();
        var serviceProvider = new Mock<IServiceProvider>();
        var options = new ApiVersioningOptions() { DefaultApiVersion = new( 2.0 ) };

        serviceProvider.Setup( sp => sp.GetService( typeof( IOptions<ApiVersioningOptions> ) ) )
                       .Returns( Options.Create( options ) );
        routeBuilder.SetupGet( rb => rb.ServiceProvider ).Returns( serviceProvider.Object );

        var apiBuilder = new VersionedApiBuilder( routeBuilder.Object );

        // act
        var model = apiBuilder.Build();

        // assert
        model.Should().BeEquivalentTo( new ApiVersionModel( new ApiVersion( 2.0 ) ) );
    }
}