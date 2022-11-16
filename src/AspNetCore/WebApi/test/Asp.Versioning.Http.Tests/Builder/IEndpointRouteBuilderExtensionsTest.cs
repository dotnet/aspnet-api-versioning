// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Builder;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

public class IEndpointRouteBuilderExtensionsTest
{
    [Fact]
    public void new_api_version_set_should_use_name()
    {
        // arrange
        var services = new ServiceCollection();

        services.AddControllers();
        services.AddApiVersioning();

        var endpoints = Mock.Of<IEndpointRouteBuilder>();

        Mock.Get( endpoints )
            .Setup( e => e.ServiceProvider )
            .Returns( services.BuildServiceProvider() );

        // act
        var versionSet = endpoints.NewApiVersionSet( "Test" ).Build();

        // assert
        versionSet.Name.Should().Be( "Test" );
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
        var group = app.MapGroup( "Test" );

        // act
        var withApiVersionSet = () => group.WithApiVersionSet().WithApiVersionSet();

        // assert
        withApiVersionSet.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void with_api_version_set_should_not_allow_nesting()
    {
        // arrange
        var builder = WebApplication.CreateBuilder();
        var services = builder.Services;

        services.AddControllers();
        services.AddApiVersioning();

        var app = builder.Build();
        var g1 = app.MapGroup( "Root" ).WithApiVersionSet();
        var g2 = g1.MapGroup( "Test" );

        // act
        var withApiVersionSet = () => g2.WithApiVersionSet();

        // assert
        withApiVersionSet.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void map_api_group_should_not_be_allowed_multiple_times()
    {
        // arrange
        var builder = WebApplication.CreateBuilder();
        var services = builder.Services;

        services.AddControllers();
        services.AddApiVersioning();

        var app = builder.Build();

        // act
        var mapApiGroup = () => app.MapApiGroup().MapApiGroup();

        // assert
        mapApiGroup.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void map_api_group_should_not_allow_nesting()
    {
        // arrange
        var builder = WebApplication.CreateBuilder();
        var services = builder.Services;

        services.AddControllers();
        services.AddApiVersioning();

        var app = builder.Build();
        var g1 = app.MapApiGroup();
        var g2 = g1.MapGroup( "Test" );

        // act
        var mapApiGroup = () => g2.MapApiGroup();

        // assert
        mapApiGroup.Should().Throw<InvalidOperationException>();
    }
}