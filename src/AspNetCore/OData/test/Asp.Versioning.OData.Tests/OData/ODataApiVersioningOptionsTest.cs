// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData;

using Microsoft.AspNetCore.OData.Batch;
using Microsoft.Extensions.DependencyInjection;

public class ODataApiVersioningOptionsTest
{
    [Fact]
    public void has_configurations_should_be_false_when_empty()
    {
        // arrange
        var builder = new VersionedODataModelBuilder(
            Mock.Of<IODataApiVersionCollectionProvider>(),
            Enumerable.Empty<IModelConfiguration>() );

        // act
        var options = new ODataApiVersioningOptions( builder );

        // assert
        options.HasConfigurations.Should().BeFalse();
    }

    [Fact]
    public void has_configurations_should_be_true_when_nonempty()
    {
        // arrange
        var builder = new VersionedODataModelBuilder(
            Mock.Of<IODataApiVersionCollectionProvider>(),
            Enumerable.Empty<IModelConfiguration>() );

        // act
        var options = new ODataApiVersioningOptions( builder ).AddRouteComponents();

        // assert
        options.HasConfigurations.Should().BeTrue();
    }

    [Fact]
    public void add_route_components_should_add_prefix_only()
    {
        // arrange
        var builder = new VersionedODataModelBuilder(
            Mock.Of<IODataApiVersionCollectionProvider>(),
            Enumerable.Empty<IModelConfiguration>() );
        var options = new ODataApiVersioningOptions( builder );

        // act
        options.AddRouteComponents( "Test" );

        // assert
        options.Configurations.ContainsKey( "Test" );
    }

    [Fact]
    public void add_route_components_should_add_configure_action_only()
    {
        // arrange
        var builder = new VersionedODataModelBuilder(
            Mock.Of<IODataApiVersionCollectionProvider>(),
            Enumerable.Empty<IModelConfiguration>() );
        var options = new ODataApiVersioningOptions( builder );
        var configureAction = Mock.Of<Action<IServiceCollection>>();
        var services = new ServiceCollection();

        // act
        options.AddRouteComponents( configureAction );
        options.Configurations[string.Empty]( services );

        // assert
        Mock.Get( configureAction ).Verify( f => f( services ), Times.Once() );
    }

    [Fact]
    public void add_route_components_should_add_batch_handler()
    {
        // arrange
        var builder = new VersionedODataModelBuilder(
            Mock.Of<IODataApiVersionCollectionProvider>(),
            Enumerable.Empty<IModelConfiguration>() );
        var options = new ODataApiVersioningOptions( builder );
        var handler = new DefaultODataBatchHandler();
        var services = new ServiceCollection();

        // act
        options.AddRouteComponents( "Test", handler );

        // assert
        options.Configurations["Test"]( services );
        services[0].ImplementationInstance.Should().BeSameAs( handler );
    }
}