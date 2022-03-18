// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Builder;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

public class VersionedEndpointBuilderExtensionsTest
{
    [Fact]
    public void report_api_versions_should_set_property_to_true()
    {
        // arrange
        var builder = Mock.Of<IVersionedApiBuilder>();

        // act
        builder.ReportApiVersions();

        // assert
        builder.ReportApiVersions.Should().BeTrue();
    }

    [Fact]
    public void map_get_should_map_request_delegate()
    {
        // arrange
        var builder = NewVersionedEndpointBuilder();
        RequestDelegate handler = c => Task.CompletedTask;

        // act
        builder.MapGet( "test", handler );

        // assert
        Mock.Get( builder ).Verify( b => b.MapMethods( "test", new[] { "GET" }, handler ) );
    }

    [Fact]
    public void map_get_should_map_delegate()
    {
        // arrange
        var builder = NewVersionedEndpointBuilder();
        Delegate handler = () => { };

        // act
        builder.MapGet( "test", handler );

        // assert
        Mock.Get( builder ).Verify( b => b.MapMethods( "test", new[] { "GET" }, handler ) );
    }

    [Fact]
    public void map_post_should_map_request_delegate()
    {
        // arrange
        var builder = NewVersionedEndpointBuilder();
        RequestDelegate handler = c => Task.CompletedTask;

        // act
        builder.MapPost( "test", handler );

        // assert
        Mock.Get( builder ).Verify( b => b.MapMethods( "test", new[] { "POST" }, handler ) );
    }

    [Fact]
    public void map_post_should_map_delegate()
    {
        // arrange
        var builder = NewVersionedEndpointBuilder();
        Delegate handler = () => { };

        // act
        builder.MapPost( "test", handler );

        // assert
        Mock.Get( builder ).Verify( b => b.MapMethods( "test", new[] { "POST" }, handler ) );
    }

    [Fact]
    public void map_put_should_map_request_delegate()
    {
        // arrange
        var builder = NewVersionedEndpointBuilder();
        RequestDelegate handler = c => Task.CompletedTask;

        // act
        builder.MapPut( "test", handler );

        // assert
        Mock.Get( builder ).Verify( b => b.MapMethods( "test", new[] { "PUT" }, handler ) );
    }

    [Fact]
    public void map_put_should_map_delegate()
    {
        // arrange
        var builder = NewVersionedEndpointBuilder();
        Delegate handler = () => { };

        // act
        builder.MapPut( "test", handler );

        // assert
        Mock.Get( builder ).Verify( b => b.MapMethods( "test", new[] { "PUT" }, handler ) );
    }

    [Fact]
    public void map_delete_should_map_request_delegate()
    {
        // arrange
        var builder = NewVersionedEndpointBuilder();
        RequestDelegate handler = c => Task.CompletedTask;

        // act
        builder.MapDelete( "test", handler );

        // assert
        Mock.Get( builder ).Verify( b => b.MapMethods( "test", new[] { "DELETE" }, handler ) );
    }

    [Fact]
    public void map_delete_should_map_delegate()
    {
        // arrange
        var builder = NewVersionedEndpointBuilder();
        Delegate handler = () => { };

        // act
        builder.MapDelete( "test", handler );

        // assert
        Mock.Get( builder ).Verify( b => b.MapMethods( "test", new[] { "DELETE" }, handler ) );
    }

    [Fact]
    public void map_patch_should_map_request_delegate()
    {
        // arrange
        var builder = NewVersionedEndpointBuilder();
        RequestDelegate handler = c => Task.CompletedTask;

        // act
        builder.MapPatch( "test", handler );

        // assert
        Mock.Get( builder ).Verify( b => b.MapMethods( "test", new[] { "PATCH" }, handler ) );
    }

    [Fact]
    public void map_patch_should_map_delegate()
    {
        // arrange
        var builder = NewVersionedEndpointBuilder();
        Delegate handler = () => { };

        // act
        builder.MapPatch( "test", handler );

        // assert
        Mock.Get( builder ).Verify( b => b.MapMethods( "test", new[] { "PATCH" }, handler ) );
    }

    private static VersionedEndpointBuilder NewVersionedEndpointBuilder() =>
        new Mock<VersionedEndpointBuilder>( Mock.Of<IVersionedApiBuilder>() ).Object;
}