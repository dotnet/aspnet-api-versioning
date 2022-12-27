// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApiExplorer;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Buffers;

public class ODataApiDescriptionProviderTest
{
    [Fact]
    public void odata_api_explorer_should_group_and_order_descriptions_on_providers_executed()
    {
        // arrange
        var builder = new WebHostBuilder()
            .ConfigureServices(
                services =>
                {
                    services.AddControllers()
                            .AddOData(
                                options =>
                                {
                                    options.Count().Select().OrderBy();
                                    options.RouteOptions.EnableKeyInParenthesis = false;
                                    options.RouteOptions.EnableNonParenthesisForEmptyParameterFunction = true;
                                    options.RouteOptions.EnableQualifiedOperationCall = false;
                                    options.RouteOptions.EnableUnqualifiedOperationCall = true;
                                } );

                    services.AddApiVersioning()
                            .AddOData( options => options.AddRouteComponents( "api" ) )
                            .AddODataApiExplorer( options => options.GroupNameFormat = "'v'VVV" );

                    services.TryAddEnumerable( ServiceDescriptor.Transient<IApplicationModelProvider, TestApiExplorerApplicationModelProvider>() );
                } )
            .Configure( app => app.UseRouting().UseEndpoints( endpoints => endpoints.MapControllers() ) );
        var host = builder.Build();
        var serviceProvider = host.Services;

        // act
        var groups = serviceProvider.GetRequiredService<IApiDescriptionGroupCollectionProvider>()
                                    .ApiDescriptionGroups
                                    .Items
                                    .OrderBy( i => i.GroupName )
                                    .ToArray();

        // assert
        groups.Length.Should().Be( 4 );
        AssertVersion0_9( groups[0] );
        AssertVersion1( groups[1] );
        AssertVersion2( groups[2] );
        AssertVersion3( groups[3] );
    }

    [Theory]
    [InlineData( ODataMetadataOptions.ServiceDocument )]
    [InlineData( ODataMetadataOptions.Metadata )]
    [InlineData( ODataMetadataOptions.All )]
    public void odata_api_explorer_should_explore_metadata_routes( ODataMetadataOptions metadataOptions )
    {
        // arrange
        var builder = new WebHostBuilder()
            .ConfigureServices(
                services =>
                {
                    services.AddControllers()
                            .AddOData(
                                options =>
                                {
                                    options.Count().Select().OrderBy();
                                    options.RouteOptions.EnableKeyInParenthesis = false;
                                    options.RouteOptions.EnableNonParenthesisForEmptyParameterFunction = true;
                                    options.RouteOptions.EnableQualifiedOperationCall = false;
                                    options.RouteOptions.EnableUnqualifiedOperationCall = true;
                                } );

                    services.AddApiVersioning()
                            .AddOData( options => options.AddRouteComponents( "api" ) )
                            .AddODataApiExplorer( options => options.MetadataOptions = metadataOptions );

                    services.TryAddEnumerable( ServiceDescriptor.Transient<IApplicationModelProvider, TestApiExplorerApplicationModelProvider>() );
                } )
            .Configure( app => app.UseRouting().UseEndpoints( endpoints => endpoints.MapControllers() ) );
        var host = builder.Build();
        var serviceProvider = host.Services;

        // act
        var groups = serviceProvider.GetRequiredService<IApiDescriptionGroupCollectionProvider>()
                                    .ApiDescriptionGroups
                                    .Items
                                    .OrderBy( i => i.GroupName )
                                    .ToArray();

        // assert
        for ( var i = 0; i < groups.Length; i++ )
        {
            var group = groups[i];

            if ( metadataOptions.HasFlag( ODataMetadataOptions.ServiceDocument ) )
            {
                group.Items.Should().Contain( item => item.RelativePath == "api" );
            }

            if ( metadataOptions.HasFlag( ODataMetadataOptions.Metadata ) )
            {
                group.Items.Should().Contain( item => item.RelativePath == "api/$metadata" );
            }
        }
    }

    private readonly ITestOutputHelper console;

    public ODataApiDescriptionProviderTest( ITestOutputHelper console ) => this.console = console;

    private void AssertVersion0_9( ApiDescriptionGroup group )
    {
        const string GroupName = "v0.9";
        var items = group.Items.OrderBy( i => i.RelativePath ).ThenBy( i => i.HttpMethod ).ToArray();

        PrintGroup( items );
        group.GroupName.Should().Be( GroupName );
        items.Should().BeEquivalentTo(
            new[]
            {
                new { HttpMethod = "GET", GroupName, RelativePath = "api/GetSalesTaxRate(PostalCode={postalCode})" },
                new { HttpMethod = "GET", GroupName, RelativePath = "api/Orders/{key}" },
                new { HttpMethod = "GET", GroupName, RelativePath = "api/People/{key}" },
            },
            options => options.ExcludingMissingMembers() );
    }

    private void AssertVersion1( ApiDescriptionGroup group )
    {
        const string GroupName = "v1";
        var items = group.Items.OrderBy( i => i.RelativePath ).ThenBy( i => i.HttpMethod ).ToArray();

        PrintGroup( items );
        group.GroupName.Should().Be( GroupName );
        items.Should().BeEquivalentTo(
            new[]
            {
                new { HttpMethod = "GET", GroupName, RelativePath = "api/Books" },
                new { HttpMethod = "GET", GroupName, RelativePath = "api/Books/{id}" },
                new { HttpMethod = "GET", GroupName, RelativePath = "api/GetSalesTaxRate(PostalCode={postalCode})" },
                new { HttpMethod = "POST", GroupName, RelativePath = "api/Orders" },
                new { HttpMethod = "GET", GroupName, RelativePath = "api/Orders/{key}" },
                new { HttpMethod = "GET", GroupName, RelativePath = "api/Orders/MostExpensive" },
                new { HttpMethod = "GET", GroupName, RelativePath = "api/People/{key}" },
            },
            options => options.ExcludingMissingMembers() );

        AssertQueryOptionWithoutOData( items[0], "filter", "author", "published" );
    }

    private void AssertVersion2( ApiDescriptionGroup group )
    {
        const string GroupName = "v2";
        var items = group.Items.OrderBy( i => i.RelativePath ).ThenBy( i => i.HttpMethod ).ToArray();

        PrintGroup( items );
        group.GroupName.Should().Be( GroupName );
        items.Should().BeEquivalentTo(
            new[]
            {
                new { HttpMethod = "GET", GroupName, RelativePath = "api/GetSalesTaxRate(PostalCode={postalCode})" },
                new { HttpMethod = "GET", GroupName, RelativePath = "api/Orders" },
                new { HttpMethod = "POST", GroupName, RelativePath = "api/Orders" },
                new { HttpMethod = "GET", GroupName, RelativePath = "api/Orders/{key}" },
                new { HttpMethod = "PATCH", GroupName, RelativePath = "api/Orders/{key}" },
                new { HttpMethod = "POST", GroupName, RelativePath = "api/Orders/{key}/Rate" },
                new { HttpMethod = "GET", GroupName, RelativePath = "api/Orders/$count" },
                new { HttpMethod = "GET", GroupName, RelativePath = "api/Orders/MostExpensive" },
                new { HttpMethod = "GET", GroupName, RelativePath = "api/People" },
                new { HttpMethod = "GET", GroupName, RelativePath = "api/People/{key}" },
                new { HttpMethod = "GET", GroupName, RelativePath = "api/People/$count" },
                new { HttpMethod = "GET", GroupName, RelativePath = "api/People/NewHires(Since={since})" },
            },
            options => options.ExcludingMissingMembers() );
    }

    private void AssertVersion3( ApiDescriptionGroup group )
    {
        const string GroupName = "v3";
        var items = group.Items.OrderBy( i => i.RelativePath ).ThenBy( i => i.HttpMethod ).ToArray();
        var expected = new[]
        {
            new { HttpMethod = "GET", GroupName, RelativePath = "api/GetSalesTaxRate(PostalCode={postalCode})" },
            new { HttpMethod = "GET", GroupName, RelativePath = "api/Orders" },
            new { HttpMethod = "POST", GroupName, RelativePath = "api/Orders" },
            new { HttpMethod = "DELETE", GroupName, RelativePath = "api/Orders/{key}" },
            new { HttpMethod = "GET", GroupName, RelativePath = "api/Orders/{key}" },
            new { HttpMethod = "PATCH", GroupName, RelativePath = "api/Orders/{key}" },
            new { HttpMethod = "POST", GroupName, RelativePath = "api/Orders/{key}/Rate" },
            new { HttpMethod = "GET", GroupName, RelativePath = "api/Orders/$count" },
            new { HttpMethod = "GET", GroupName, RelativePath = "api/Orders/MostExpensive" },
            new { HttpMethod = "GET", GroupName, RelativePath = "api/People" },
            new { HttpMethod = "POST", GroupName, RelativePath = "api/People" },
            new { HttpMethod = "GET", GroupName, RelativePath = "api/People/{key}" },
            new { HttpMethod = "POST", GroupName, RelativePath = "api/People/{key}/Promote" },
            new { HttpMethod = "GET", GroupName, RelativePath = "api/People/$count" },
            new { HttpMethod = "GET", GroupName, RelativePath = "api/People/NewHires(Since={since})" },
            new { HttpMethod = "GET", GroupName, RelativePath = "api/Products" },
            new { HttpMethod = "POST", GroupName, RelativePath = "api/Products" },
            new { HttpMethod = "DELETE", GroupName, RelativePath = "api/Products/{key}" },
            new { HttpMethod = "GET", GroupName, RelativePath = "api/Products/{key}" },
            new { HttpMethod = "GET", GroupName, RelativePath = "api/Products/$count" },
            new { HttpMethod = "PATCH", GroupName, RelativePath = "api/Products/{key}" },
            new { HttpMethod = "PUT", GroupName, RelativePath = "api/Products/{key}" },
            new { HttpMethod = "GET", GroupName, RelativePath = "api/Products/{key}/Supplier" },
            new { HttpMethod = "DELETE", GroupName, RelativePath = "api/Products/{key}/supplier/{relatedKey}/$ref" },
            new { HttpMethod = "GET", GroupName, RelativePath = "api/Products/{key}/supplier/$ref" },
            new { HttpMethod = "PUT", GroupName, RelativePath = "api/Products/{key}/supplier/$ref" },
            new { HttpMethod = "GET", GroupName, RelativePath = "api/Suppliers" },
            new { HttpMethod = "POST", GroupName, RelativePath = "api/Suppliers" },
            new { HttpMethod = "DELETE", GroupName, RelativePath = "api/Suppliers/{key}" },
            new { HttpMethod = "GET", GroupName, RelativePath = "api/Suppliers/{key}" },
            new { HttpMethod = "GET", GroupName, RelativePath = "api/Suppliers/$count" },
            new { HttpMethod = "PATCH", GroupName, RelativePath = "api/Suppliers/{key}" },
            new { HttpMethod = "PUT", GroupName, RelativePath = "api/Suppliers/{key}" },
            new { HttpMethod = "GET", GroupName, RelativePath = "api/Suppliers/{key}/Products" },
            new { HttpMethod = "DELETE", GroupName, RelativePath = "api/Suppliers/{key}/products/{relatedKey}/$ref" },
            new { HttpMethod = "PUT", GroupName, RelativePath = "api/Suppliers/{key}/products/$ref" },
        };

        PrintGroup( items );
        group.GroupName.Should().Be( GroupName );
        items.Should().BeEquivalentTo( expected, options => options.ExcludingMissingMembers() );
    }

    private static void AssertQueryOptionWithoutOData( ApiDescription description, string name, string property, params string[] otherProperties )
    {
        var parameter = description.ParameterDescriptions.Single( p => p.Name == name );
        var count = otherProperties.Length + 1;
        string suffix;

        if ( count == 1 )
        {
            suffix = property;
        }
        else
        {
            var pool = ArrayPool<string>.Shared;
            var properties = pool.Rent( count );

            properties[0] = property;
            Array.Copy( otherProperties, 0, properties, 1, count - 1 );

            suffix = string.Join( ", ", properties, 0, count );
        }

        parameter.ModelMetadata.Description.Should().EndWith( suffix + '.' );
    }

    private void PrintGroup( IReadOnlyList<ApiDescription> items )
    {
        for ( var i = 0; i < items.Count; i++ )
        {
            var item = items[i];
            console.WriteLine( $"[{item.GroupName}] {item.HttpMethod} {item.RelativePath}" );
        }
    }

#pragma warning disable CA1812

    private sealed class TestApiExplorerApplicationModelProvider : IApplicationModelProvider
    {
        public int Order { get; }

        public void OnProvidersExecuted( ApplicationModelProviderContext context )
        {
            var controllers = context.Result.Controllers;

            for ( var i = 0; i < controllers.Count; i++ )
            {
                var controller = controllers[i];
                controller.ApiExplorer.IsVisible = true;
            }
        }

        public void OnProvidersExecuting( ApplicationModelProviderContext context ) { }
    }
}