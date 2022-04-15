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
}