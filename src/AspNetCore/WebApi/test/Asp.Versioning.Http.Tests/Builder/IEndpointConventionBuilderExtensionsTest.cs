// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Builder;

using Asp.Versioning.Conventions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using static Asp.Versioning.ApiVersionProviderOptions;

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

    [Fact]
    public void with_api_version_set_should_not_be_allowed_multiple_times()
    {
        // arrange
        var builder = WebApplication.CreateBuilder();
        var services = builder.Services;

        services.AddControllers();
        services.AddApiVersioning();

        var app = builder.Build();
        var versionSet = new ApiVersionSetBuilder( default ).Build();
        var get = app.MapGet( "/", () => Results.Ok() );
        IEndpointRouteBuilder endpoints = app;

        get.WithApiVersionSet( versionSet );
        get.WithApiVersionSet( versionSet );

        // act
        var build = () => endpoints.DataSources.Single().Endpoints;

        // assert
        build.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void with_api_version_set_should_not_override_existing_metadata()
    {
        // arrange
        var builder = WebApplication.CreateBuilder();
        var services = builder.Services;

        services.AddControllers();
        services.AddApiVersioning();

        var app = builder.Build();
        var versionSet = new ApiVersionSetBuilder( default ).Build();
        var group = app.NewVersionedApi();
        var get = group.MapGet( "/", () => Results.Ok() );
        IEndpointRouteBuilder endpoints = app;

        get.WithApiVersionSet( versionSet );

        // act
        var build = () => endpoints.DataSources.Single().Endpoints;

        // assert
        build.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void report_api_versions_should_add_convention()
    {
        // arrange
        var conventions = new Mock<IEndpointConventionBuilder>();
        var reportApiVersions = default( IReportApiVersions );

        conventions.Setup( b => b.Add( It.IsAny<Action<EndpointBuilder>>() ) )
                   .Callback( ( Action<EndpointBuilder> callback ) =>
                   {
                       var endpoint = Mock.Of<EndpointBuilder>();
                       var versionSet = new ApiVersionSetBuilder( default ).Build();
                       endpoint.Metadata.Add( versionSet );
                       callback( endpoint );
                       reportApiVersions = endpoint.Metadata.OfType<IReportApiVersions>().First();
                   } );

        var route = new RouteHandlerBuilder( new[] { conventions.Object } );

        // act
        route.ReportApiVersions();

        // assert
        reportApiVersions.Should().NotBeNull();
    }

    [Fact]
    public void is_api_version_neutral_should_add_convention()
    {
        // arrange
        var conventions = new Mock<IEndpointConventionBuilder>();
        var versionNeutral = default( IApiVersionNeutral );

        conventions.Setup( b => b.Add( It.IsAny<Action<EndpointBuilder>>() ) )
                   .Callback( ( Action<EndpointBuilder> callback ) =>
                   {
                       var endpoint = Mock.Of<EndpointBuilder>();
                       var versionSet = new ApiVersionSetBuilder( default ).Build();
                       endpoint.Metadata.Add( versionSet );
                       callback( endpoint );
                       versionNeutral = endpoint.Metadata.OfType<IApiVersionNeutral>().First();
                   } );

        var route = new RouteHandlerBuilder( new[] { conventions.Object } );

        // act
        route.IsApiVersionNeutral();

        // assert
        versionNeutral.Should().NotBeNull();
    }

    [Fact]
    public void has_api_version_should_add_convention()
    {
        // arrange
        var conventions = new Mock<IEndpointConventionBuilder>();
        var provider = default( IApiVersionProvider );

        conventions.Setup( b => b.Add( It.IsAny<Action<EndpointBuilder>>() ) )
                   .Callback( ( Action<EndpointBuilder> callback ) =>
                   {
                       var endpoint = Mock.Of<EndpointBuilder>();
                       var versionSet = new ApiVersionSetBuilder( default ).Build();
                       endpoint.Metadata.Add( versionSet );
                       callback( endpoint );
                       provider = endpoint.Metadata.OfType<IApiVersionProvider>().First();
                   } );

        var route = new RouteHandlerBuilder( new[] { conventions.Object } );

        // act
        route.HasApiVersion( 1.0 );

        // assert
        provider.Should().BeEquivalentTo(
            new
            {
                Options = None,
                Versions = new[] { new ApiVersion( 1.0 ) },
            } );
    }

    [Fact]
    public void has_api_version_should_propagate_to_version_set()
    {
        // arrange
        var conventions = new Mock<IEndpointConventionBuilder>();
        var versionSet = new ApiVersionSetBuilder( default ).Build();

        conventions.Setup( b => b.Add( It.IsAny<Action<EndpointBuilder>>() ) )
                   .Callback( ( Action<EndpointBuilder> callback ) =>
                   {
                       var endpoint = Mock.Of<EndpointBuilder>();
                       endpoint.Metadata.Add( versionSet );
                       callback( endpoint );
                   } );

        var route = new RouteHandlerBuilder( new[] { conventions.Object } );

        // act
        route.HasApiVersion( 1.0 );

        // assert
        versionSet.Build( new() ).SupportedApiVersions.Single().Should().Be( new ApiVersion( 1.0 ) );
    }

    [Fact]
    public void has_deprecated_api_version_should_add_convention()
    {
        // arrange
        var conventions = new Mock<IEndpointConventionBuilder>();
        var provider = default( IApiVersionProvider );

        conventions.Setup( b => b.Add( It.IsAny<Action<EndpointBuilder>>() ) )
                   .Callback( ( Action<EndpointBuilder> callback ) =>
                   {
                       var endpoint = Mock.Of<EndpointBuilder>();
                       var versionSet = new ApiVersionSetBuilder( default ).Build();
                       endpoint.Metadata.Add( versionSet );
                       callback( endpoint );
                       provider = endpoint.Metadata.OfType<IApiVersionProvider>().First();
                   } );

        var route = new RouteHandlerBuilder( new[] { conventions.Object } );

        // act
        route.HasDeprecatedApiVersion( 0.9 );

        // assert
        provider.Should().BeEquivalentTo(
            new
            {
                Options = Deprecated,
                Versions = new[] { new ApiVersion( 0.9 ) },
            } );
    }

    [Fact]
    public void has_deprecated_api_version_should_propagate_to_version_set()
    {
        // arrange
        var conventions = new Mock<IEndpointConventionBuilder>();
        var versionSet = new ApiVersionSetBuilder( default ).Build();

        conventions.Setup( b => b.Add( It.IsAny<Action<EndpointBuilder>>() ) )
                   .Callback( ( Action<EndpointBuilder> callback ) =>
                   {
                       var endpoint = Mock.Of<EndpointBuilder>();
                       endpoint.Metadata.Add( versionSet );
                       callback( endpoint );
                   } );

        var route = new RouteHandlerBuilder( new[] { conventions.Object } );

        // act
        route.HasDeprecatedApiVersion( 0.9 );

        // assert
        versionSet.Build( new() ).DeprecatedApiVersions.Single().Should().Be( new ApiVersion( 0.9 ) );
    }

    [Fact]
    public void advertises_api_version_should_add_convention()
    {
        // arrange
        var conventions = new Mock<IEndpointConventionBuilder>();
        var provider = default( IApiVersionProvider );

        conventions.Setup( b => b.Add( It.IsAny<Action<EndpointBuilder>>() ) )
                   .Callback( ( Action<EndpointBuilder> callback ) =>
                   {
                       var endpoint = Mock.Of<EndpointBuilder>();
                       var versionSet = new ApiVersionSetBuilder( default ).Build();
                       endpoint.Metadata.Add( versionSet );
                       callback( endpoint );
                       provider = endpoint.Metadata.OfType<IApiVersionProvider>().First();
                   } );

        var route = new RouteHandlerBuilder( new[] { conventions.Object } );

        // act
        route.AdvertisesApiVersion( 42.0 );

        // assert
        provider.Should().BeEquivalentTo(
            new
            {
                Options = Advertised,
                Versions = new[] { new ApiVersion( 42.0 ) },
            } );
    }

    [Fact]
    public void advertises_api_version_should_propagate_to_version_set()
    {
        // arrange
        var conventions = new Mock<IEndpointConventionBuilder>();
        var versionSet = new ApiVersionSetBuilder( default ).Build();

        conventions.Setup( b => b.Add( It.IsAny<Action<EndpointBuilder>>() ) )
                   .Callback( ( Action<EndpointBuilder> callback ) =>
                   {
                       var endpoint = Mock.Of<EndpointBuilder>();
                       endpoint.Metadata.Add( versionSet );
                       callback( endpoint );
                   } );

        var route = new RouteHandlerBuilder( new[] { conventions.Object } );

        // act
        route.AdvertisesApiVersion( 42.0 );

        // assert
        versionSet.Build( new() ).SupportedApiVersions.Single().Should().Be( new ApiVersion( 42.0 ) );
    }

    [Fact]
    public void advertises_deprecated_api_version_should_add_convention()
    {
        // arrange
        var conventions = new Mock<IEndpointConventionBuilder>();
        var provider = default( IApiVersionProvider );

        conventions.Setup( b => b.Add( It.IsAny<Action<EndpointBuilder>>() ) )
                   .Callback( ( Action<EndpointBuilder> callback ) =>
                   {
                       var endpoint = Mock.Of<EndpointBuilder>();
                       var versionSet = new ApiVersionSetBuilder( default ).Build();
                       endpoint.Metadata.Add( versionSet );
                       callback( endpoint );
                       provider = endpoint.Metadata.OfType<IApiVersionProvider>().First();
                   } );

        var route = new RouteHandlerBuilder( new[] { conventions.Object } );

        // act
        route.AdvertisesDeprecatedApiVersion( 42.0, "rc" );

        // assert
        provider.Should().BeEquivalentTo(
            new
            {
                Options = Advertised | Deprecated,
                Versions = new[] { new ApiVersion( 42.0, "rc" ) },
            } );
    }

    [Fact]
    public void advertises_deprecated_api_version_should_propagate_to_version_set()
    {
        // arrange
        var conventions = new Mock<IEndpointConventionBuilder>();
        var versionSet = new ApiVersionSetBuilder( default ).Build();

        conventions.Setup( b => b.Add( It.IsAny<Action<EndpointBuilder>>() ) )
                   .Callback( ( Action<EndpointBuilder> callback ) =>
                   {
                       var endpoint = Mock.Of<EndpointBuilder>();
                       endpoint.Metadata.Add( versionSet );
                       callback( endpoint );
                   } );

        var route = new RouteHandlerBuilder( new[] { conventions.Object } );

        // act
        route.AdvertisesDeprecatedApiVersion( 42.0, "rc" );

        // assert
        versionSet.Build( new() ).DeprecatedApiVersions.Single().Should().Be( new ApiVersion( 42.0, "rc" ) );
    }

    [Fact]
    public void map_to_api_version_should_add_convention()
    {
        // arrange
        var conventions = new Mock<IEndpointConventionBuilder>();
        var provider = default( IApiVersionProvider );

        conventions.Setup( b => b.Add( It.IsAny<Action<EndpointBuilder>>() ) )
                   .Callback( ( Action<EndpointBuilder> callback ) =>
                   {
                       var endpoint = Mock.Of<EndpointBuilder>();
                       var versionSet = new ApiVersionSetBuilder( default ).Build();
                       endpoint.Metadata.Add( versionSet );
                       callback( endpoint );
                       provider = endpoint.Metadata.OfType<IApiVersionProvider>().First();
                   } );

        var route = new RouteHandlerBuilder( new[] { conventions.Object } );

        // act
        route.MapToApiVersion( 2.0 );

        // assert
        provider.Should().BeEquivalentTo(
            new
            {
                Options = Mapped,
                Versions = new[] { new ApiVersion( 2.0 ) },
            } );
    }

    [Fact]
    public void map_to_api_version_should_throw_exception_without_version_set()
    {
        // arrange
        var conventions = new Mock<IEndpointConventionBuilder>();

        conventions.Setup( b => b.Add( It.IsAny<Action<EndpointBuilder>>() ) )
                   .Callback( ( Action<EndpointBuilder> callback ) => callback( Mock.Of<EndpointBuilder>() ) );

        var route = new RouteHandlerBuilder( new[] { conventions.Object } );

        // act
        var mapToApiVersion = () => route.MapToApiVersion( 2.0 );

        // assert
        mapToApiVersion.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void has_api_version_should_throw_exception_without_version_set()
    {
        // arrange
        var conventions = new Mock<IEndpointConventionBuilder>();

        conventions.Setup( b => b.Add( It.IsAny<Action<EndpointBuilder>>() ) )
                   .Callback( ( Action<EndpointBuilder> callback ) => callback( Mock.Of<EndpointBuilder>() ) );

        var route = new RouteHandlerBuilder( new[] { conventions.Object } );

        // act
        var hasApiVersion = () => route.HasApiVersion( 2.0 );

        // assert
        hasApiVersion.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void has_deprecated_api_version_should_throw_exception_without_version_set()
    {
        // arrange
        var conventions = new Mock<IEndpointConventionBuilder>();

        conventions.Setup( b => b.Add( It.IsAny<Action<EndpointBuilder>>() ) )
                   .Callback( ( Action<EndpointBuilder> callback ) => callback( Mock.Of<EndpointBuilder>() ) );

        var route = new RouteHandlerBuilder( new[] { conventions.Object } );

        // act
        var hasDeprecatedApiVersion = () => route.HasDeprecatedApiVersion( 2.0 );

        // assert
        hasDeprecatedApiVersion.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void advertises_api_version_should_throw_exception_without_version_set()
    {
        // arrange
        var conventions = new Mock<IEndpointConventionBuilder>();

        conventions.Setup( b => b.Add( It.IsAny<Action<EndpointBuilder>>() ) )
                   .Callback( ( Action<EndpointBuilder> callback ) => callback( Mock.Of<EndpointBuilder>() ) );

        var route = new RouteHandlerBuilder( new[] { conventions.Object } );

        // act
        var advertisesApiVersion = () => route.AdvertisesApiVersion( 2.0 );

        // assert
        advertisesApiVersion.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void advertises_deprecated_api_version_should_throw_exception_without_version_set()
    {
        // arrange
        var conventions = new Mock<IEndpointConventionBuilder>();

        conventions.Setup( b => b.Add( It.IsAny<Action<EndpointBuilder>>() ) )
                   .Callback( ( Action<EndpointBuilder> callback ) => callback( Mock.Of<EndpointBuilder>() ) );

        var route = new RouteHandlerBuilder( new[] { conventions.Object } );

        // act
        var advertisesDeprecatedApiVersion = () => route.AdvertisesDeprecatedApiVersion( 2.0 );

        // assert
        advertisesDeprecatedApiVersion.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void is_api_version_neutral_should_throw_exception_without_version_set()
    {
        // arrange
        var conventions = new Mock<IEndpointConventionBuilder>();

        conventions.Setup( b => b.Add( It.IsAny<Action<EndpointBuilder>>() ) )
                   .Callback( ( Action<EndpointBuilder> callback ) => callback( Mock.Of<EndpointBuilder>() ) );

        var route = new RouteHandlerBuilder( new[] { conventions.Object } );

        // act
        var isApiVersionNeutral = () => route.IsApiVersionNeutral();

        // assert
        isApiVersionNeutral.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void reports_api_versions_should_throw_exception_without_version_set()
    {
        // arrange
        var conventions = new Mock<IEndpointConventionBuilder>();

        conventions.Setup( b => b.Add( It.IsAny<Action<EndpointBuilder>>() ) )
                   .Callback( ( Action<EndpointBuilder> callback ) => callback( Mock.Of<EndpointBuilder>() ) );

        var route = new RouteHandlerBuilder( new[] { conventions.Object } );

        // act
        var reportsApiVersions = () => route.ReportApiVersions();

        // assert
        reportsApiVersions.Should().Throw<InvalidOperationException>();
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

            return null;
        }
    }
}