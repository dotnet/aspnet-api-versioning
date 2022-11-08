// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Builder;

using Asp.Versioning.Conventions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;

public class IEndpointConventionBuilderExtensionsTest
{
    [Fact]
    public void with_api_version_set_should_return_same_instance()
    {
        // arrange
        var before = Mock.Of<IEndpointConventionBuilder>();
        var versionSet = new ApiVersionSetBuilder( default ).Build();

        // act
        var after = before.WithApiVersionSet( versionSet );

        // assert
        after.Should().BeSameAs( before );
    }

    [Fact]
    public void with_api_version_set_should_apply_to_endpoint()
    {
        // arrange
        var conventions = new Mock<IEndpointConventionBuilder>();
        var provider = default( IApiVersionProvider );
        var dataSources = new List<EndpointDataSource>();
        var app = new Mock<IEndpointRouteBuilder>();

        conventions.Setup( b => b.Add( It.IsAny<Action<EndpointBuilder>>() ) )
                   .Callback( ( Action<EndpointBuilder> callback ) =>
                   {
                       var endpoint = Mock.Of<EndpointBuilder>();
                       callback( endpoint );
                       provider = endpoint.Metadata.OfType<IApiVersionProvider>().First();
                   } );
        app.SetupGet( a => a.ServiceProvider ).Returns( new MockServiceProvider() );
        app.SetupGet( a => a.DataSources ).Returns( dataSources );

        // act
        var versionSet = app.Object.NewApiVersionSet()
                                   .HasApiVersion( 1.0 )
                                   .HasDeprecatedApiVersion( 0.9 )
                                   .AdvertisesApiVersion( 2.0 )
                                   .AdvertisesDeprecatedApiVersion( 2.0, "beta" )
                                   .Build();

        app.Object.MapGet( "/test", () => Results.Ok() )
                  .WithApiVersionSet( versionSet );

        // assert
        var metadata = dataSources.Single()
                                  .Endpoints
                                  .Single()
                                  .Metadata
                                  .OfType<ApiVersionMetadata>()
                                  .Single();

        metadata.Should()
                .BeEquivalentTo(
                    new ApiVersionMetadata(
                        new ApiVersionModel(
                            new ApiVersion[] { new( 0.9 ), new( 1.0 ) },
                            new ApiVersion[] { new( 1.0 ) },
                            new ApiVersion[] { new( 0.9 ) },
                            new ApiVersion[] { new( 2.0 ) },
                            new ApiVersion[] { new( 2.0, "beta" ) } ),
                        ApiVersionModel.Empty ) );
    }

    [Fact]
    public void with_api_version_set_should_apply_across_endpoints()
    {
        // arrange
        var conventions = new Mock<IEndpointConventionBuilder>();
        var provider = default( IApiVersionProvider );
        var dataSources = new List<EndpointDataSource>();
        var app = new Mock<IEndpointRouteBuilder>();

        conventions.Setup( b => b.Add( It.IsAny<Action<EndpointBuilder>>() ) )
                   .Callback( ( Action<EndpointBuilder> callback ) =>
                   {
                       var endpoint = Mock.Of<EndpointBuilder>();
                       callback( endpoint );
                       provider = endpoint.Metadata.OfType<IApiVersionProvider>().First();
                   } );
        app.SetupGet( a => a.ServiceProvider ).Returns( new MockServiceProvider() );
        app.SetupGet( a => a.DataSources ).Returns( dataSources );

        // act
        var versionSet = app.Object.NewApiVersionSet()
                                   .HasApiVersion( 1.0 )
                                   .HasApiVersion( 2.0 )
                                   .Build();

        app.Object.MapGet( "/test", () => Results.Ok() )
                  .WithApiVersionSet( versionSet )
                  .MapToApiVersion( 1.0 )
                  .HasDeprecatedApiVersion( 0.9 );

        app.Object.MapGet( "/test", () => Results.Ok() )
                  .WithApiVersionSet( versionSet )
                  .MapToApiVersion( 2.0 );

        // assert
        var endpoints = dataSources.Single().Endpoints;

        endpoints[0].Metadata
                    .OfType<ApiVersionMetadata>()
                    .Single()
                    .Should()
                    .BeEquivalentTo(
                        new ApiVersionMetadata(
                            new ApiVersionModel(
                                new ApiVersion[] { new( 1.0 ), new( 2.0 ) },
                                new ApiVersion[] { new( 1.0 ), new( 2.0 ) },
                                Array.Empty<ApiVersion>(),
                                Array.Empty<ApiVersion>(),
                                Array.Empty<ApiVersion>() ),
                            new ApiVersionModel(
                                new ApiVersion[] { new( 0.9 ), new( 1.0 ) },
                                new ApiVersion[] { new( 1.0 ), new( 2.0 ) },
                                new ApiVersion[] { new( 0.9 ) },
                                Array.Empty<ApiVersion>(),
                                Array.Empty<ApiVersion>() ) ) );

        endpoints[1].Metadata
                   .OfType<ApiVersionMetadata>()
                   .Single()
                   .Should()
                   .BeEquivalentTo(
                       new ApiVersionMetadata(
                           new ApiVersionModel(
                               new ApiVersion[] { new( 1.0 ), new( 2.0 ) },
                               new ApiVersion[] { new( 1.0 ), new( 2.0 ) },
                               Array.Empty<ApiVersion>(),
                               Array.Empty<ApiVersion>(),
                               Array.Empty<ApiVersion>() ),
                           new ApiVersionModel(
                               new ApiVersion[] { new( 2.0 ) },
                               new ApiVersion[] { new( 1.0 ), new( 2.0 ) },
                               new ApiVersion[] { new( 0.9 ) },
                               Array.Empty<ApiVersion>(),
                               Array.Empty<ApiVersion>() ) ) );
    }

    [Fact]
    public void with_api_version_set_should_collate_grouped_endpoint()
    {
        // arrange
        var dataSources = new List<EndpointDataSource>();
        var app = new Mock<IEndpointRouteBuilder>();

        app.SetupGet( a => a.ServiceProvider ).Returns( new MockServiceProvider() );
        app.SetupGet( a => a.DataSources ).Returns( dataSources );

        // act
        app.Object.MapGroup( "/test" )
                  .WithApiVersionSet()
                  .MapGet( "/", () => Results.Ok() )
                  .HasApiVersion( 1.0 )
                  .HasDeprecatedApiVersion( 0.9 )
                  .AdvertisesApiVersion( 2.0 )
                  .AdvertisesDeprecatedApiVersion( 2.0, "beta" );

        // assert
        var metadata = dataSources.Single()
                                  .Endpoints
                                  .Single()
                                  .Metadata
                                  .OfType<ApiVersionMetadata>()
                                  .Single();

        metadata.Should()
                .BeEquivalentTo(
                    new ApiVersionMetadata(
                        ApiVersionModel.Empty,
                        new ApiVersionModel(
                            new ApiVersion[] { new( 0.9 ), new( 1.0 ) },
                            new ApiVersion[] { new( 1.0 ) },
                            new ApiVersion[] { new( 0.9 ) },
                            new ApiVersion[] { new( 2.0 ) },
                            new ApiVersion[] { new( 2.0, "beta" ) } ) ) );
    }

    [Fact]
    public void with_api_version_set_should_collate_across_grouped_endpoints()
    {
        // arrange
        var dataSources = new List<EndpointDataSource>();
        var app = new Mock<IEndpointRouteBuilder>();

        app.SetupGet( a => a.ServiceProvider ).Returns( new MockServiceProvider() );
        app.SetupGet( a => a.DataSources ).Returns( dataSources );

        // act
        var test = app.Object.MapGroup( "/test" ).WithApiVersionSet();

        test.MapGet( "/", () => Results.Ok() )
            .HasApiVersion( 1.0 )
            .HasDeprecatedApiVersion( 0.9 );

        test.MapGet( "/", () => Results.Ok() )
            .HasApiVersion( 2.0 );

        test.MapDelete( "/", () => Results.NoContent() )
            .IsApiVersionNeutral();

        // assert
        var endpoints = dataSources.Single().Endpoints;

        endpoints[0].Metadata
                    .OfType<ApiVersionMetadata>()
                    .Single()
                    .Should()
                    .BeEquivalentTo(
                        new ApiVersionMetadata(
                            ApiVersionModel.Empty,
                            new ApiVersionModel(
                                new ApiVersion[] { new( 0.9 ), new( 1.0 ) },
                                new ApiVersion[] { new( 1.0 ), new( 2.0 ) },
                                new ApiVersion[] { new( 0.9 ) },
                                Array.Empty<ApiVersion>(),
                                Array.Empty<ApiVersion>() ) ) );

        endpoints[1].Metadata
                    .OfType<ApiVersionMetadata>()
                    .Single()
                    .Should()
                    .BeEquivalentTo(
                        new ApiVersionMetadata(
                            ApiVersionModel.Empty,
                            new ApiVersionModel(
                                new ApiVersion[] { new( 2.0 ) },
                                new ApiVersion[] { new( 1.0 ), new( 2.0 ) },
                                new ApiVersion[] { new( 0.9 ) },
                                Array.Empty<ApiVersion>(),
                                Array.Empty<ApiVersion>() ) ) );

        endpoints[2].Metadata
                    .OfType<ApiVersionMetadata>()
                    .Single()
                    .Should()
                    .BeEquivalentTo( ApiVersionMetadata.Neutral );
    }

    private sealed class MockServiceProvider : IServiceProvider
    {
        private readonly IOptions<ApiVersioningOptions> options = Options.Create( new ApiVersioningOptions() );

        public object GetService( Type serviceType )
        {
            if ( typeof( IOptions<ApiVersioningOptions> ) == serviceType )
            {
                return options;
            }

            if ( typeof( IApiVersionParameterSource ) == serviceType )
            {
                return options.Value.ApiVersionReader;
            }

            if ( typeof( IApiVersionSetBuilderFactory ) == serviceType )
            {
                return new DefaultApiVersionSetBuilderFactory();
            }

            return null;
        }
    }
}