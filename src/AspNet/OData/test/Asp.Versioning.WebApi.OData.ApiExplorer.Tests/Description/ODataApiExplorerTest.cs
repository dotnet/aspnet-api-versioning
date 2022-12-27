// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Description;

using Asp.Versioning.ApiExplorer;
using Asp.Versioning.Controllers;
using Asp.Versioning.Conventions;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using static System.Net.Http.HttpMethod;

public class ODataApiExplorerTest
{
    [Theory]
    [ClassData( typeof( TestConfigurations ) )]
    public void api_descriptions_should_collate_expected_versions( HttpConfiguration configuration )
    {
        // arrange
        var apiExplorer = new ODataApiExplorer( configuration );

        // act
        var descriptions = apiExplorer.ApiDescriptions;

        // assert
        descriptions.ApiVersions.Should().Equal(
            new ApiVersion( 0, 9 ),
            new ApiVersion( 1, 0 ),
            new ApiVersion( 2, 0 ),
            new ApiVersion( 3, 0 ) );
    }

    [Theory]
    [ClassData( typeof( TestConfigurations ) )]
    public void api_descriptions_should_group_versioned_controllers( HttpConfiguration configuration )
    {
        // arrange
        var assembliesResolver = configuration.Services.GetAssembliesResolver();
        var controllerTypes = configuration.Services
                                           .GetHttpControllerTypeResolver()
                                           .GetControllerTypes( assembliesResolver )
                                           .Where( t => !typeof( MetadataController ).IsAssignableFrom( t ) );
        var apiExplorer = new ODataApiExplorer( configuration );

        // act
        var descriptions = apiExplorer.ApiDescriptions;

        // assert
        descriptions.SelectMany( g => g.ApiDescriptions )
                    .Select( d => d.ActionDescriptor.ControllerDescriptor.ControllerType )
                    .Distinct()
                    .Should()
                    .Equal( controllerTypes );
    }

    [Theory]
    [ClassData( typeof( TestConfigurations ) )]
    public void api_descriptions_should_flatten_versioned_controllers( HttpConfiguration configuration )
    {
        // arrange
        var assembliesResolver = configuration.Services.GetAssembliesResolver();
        var controllerTypes = configuration.Services
                                            .GetHttpControllerTypeResolver()
                                            .GetControllerTypes( assembliesResolver )
                                            .Where( t => !typeof( MetadataController ).IsAssignableFrom( t ) );
        var apiExplorer = new ODataApiExplorer( configuration );

        // act
        var descriptions = apiExplorer.ApiDescriptions;

        // assert
        descriptions.Flatten()
                    .Select( d => d.ActionDescriptor.ControllerDescriptor.ControllerType )
                    .Distinct()
                    .Should()
                    .Equal( controllerTypes );
    }

    [Theory]
    [ClassData( typeof( TestConfigurations ) )]
    public void api_descriptions_should_not_contain_metadata_controllers( HttpConfiguration configuration )
    {
        // arrange
        var apiExplorer = new ODataApiExplorer( configuration );

        // act
        var descriptions = apiExplorer.ApiDescriptions;

        // assert
        descriptions.Flatten()
                    .Select( d => d.ActionDescriptor.ControllerDescriptor.ControllerType )
                    .Distinct()
                    .Should()
                    .NotContain( type => typeof( MetadataController ).IsAssignableFrom( type ) );
    }

    [Theory]
    [InlineData( ODataMetadataOptions.ServiceDocument )]
    [InlineData( ODataMetadataOptions.Metadata )]
    [InlineData( ODataMetadataOptions.All )]
    public void api_descriptions_should_contain_metadata_controllers( ODataMetadataOptions metadataOptions )
    {
        // arrange
        var configuration = TestConfigurations.NewOrdersConfiguration();
        var options = new ODataApiExplorerOptions( configuration ) { MetadataOptions = metadataOptions };
        var apiExplorer = new ODataApiExplorer( configuration, options );

        // act
        var groups = apiExplorer.ApiDescriptions;

        // assert
        for ( var i = 0; i < groups.Count; i++ )
        {
            var group = groups[i];

            if ( metadataOptions.HasFlag( ODataMetadataOptions.ServiceDocument ) )
            {
                group.ApiDescriptions.Should().Contain( item => item.RelativePath == "api" );
            }

            if ( metadataOptions.HasFlag( ODataMetadataOptions.Metadata ) )
            {
                group.ApiDescriptions.Should().Contain( item => item.RelativePath == "api/$metadata" );
            }
        }
    }

    [Theory]
    [ClassData( typeof( TestConfigurations ) )]
    public void api_description_group_should_explore_v3_actions( HttpConfiguration configuration )
    {
        // arrange
        var apiVersion = new ApiVersion( 3, 0 );
        var apiExplorer = new ODataApiExplorer( configuration );
        var descriptionGroup = apiExplorer.ApiDescriptions[apiVersion];

        // act
        var descriptions = descriptionGroup.ApiDescriptions;
        var relativePaths = descriptions.Select( d => d.RelativePath ).ToArray();

        // assert
        descriptions.Should().BeEquivalentTo(
            new[]
            {
                new
                {
                    ID = $"GET{relativePaths[0]}",
                    HttpMethod = Get,
                    RelativePath = relativePaths[0],
                    Version = apiVersion,
                },
                new
                {
                    ID = $"GET{relativePaths[1]}",
                    HttpMethod = Get,
                    RelativePath = relativePaths[1],
                    Version = apiVersion,
                },
                new
                {
                    ID = $"POST{relativePaths[2]}",
                    HttpMethod = Post,
                    RelativePath = relativePaths[2],
                    Version = apiVersion,
                },
                new
                {
                    ID = $"DELETE{relativePaths[3]}",
                    HttpMethod = Delete,
                    RelativePath = relativePaths[3],
                    Version = apiVersion,
                },
            },
            options => options.ExcludingMissingMembers() );
    }

    [Fact]
    public void api_description_group_should_explore_navigation_properties()
    {
        // arrange
        var patch = new HttpMethod( "PATCH" );
        var version = new ApiVersion( 3, 0 );
        var apiExplorer = new ODataApiExplorer( TestConfigurations.NewProductAndSupplierConfiguration() );
        var descriptionGroup = apiExplorer.ApiDescriptions[version];

        // act
        var descriptions = descriptionGroup.ApiDescriptions;

        // assert
        descriptions.Should().BeEquivalentTo(
            new[]
            {
                new { HttpMethod = Get, version, RelativePath = "api/Products" },
                new { HttpMethod = Get, version, RelativePath = "api/Suppliers" },
                new { HttpMethod = Get, version, RelativePath = "api/Products/{key}" },
                new { HttpMethod = Get, version, RelativePath = "api/Suppliers/{key}" },
                new { HttpMethod = Get, version, RelativePath = "api/Products/{key}/Supplier" },
                new { HttpMethod = Get, version, RelativePath = "api/Suppliers/{key}/Products" },
                new { HttpMethod = Get, version, RelativePath = "api/Products/{key}/supplier/$ref" },
                new { HttpMethod = Put, version, RelativePath = "api/Products/{key}" },
                new { HttpMethod = Put, version, RelativePath = "api/Suppliers/{key}" },
                new { HttpMethod = Put, version, RelativePath = "api/Products/{key}/supplier/$ref" },
                new { HttpMethod = Post, version, RelativePath = "api/Products" },
                new { HttpMethod = Post, version, RelativePath = "api/Suppliers" },
                new { HttpMethod = Post, version, RelativePath = "api/Suppliers/{key}/Products/$ref" },
                new { HttpMethod = patch, version, RelativePath = "api/Products/{key}" },
                new { HttpMethod = patch, version, RelativePath = "api/Suppliers/{key}" },
                new { HttpMethod = Delete, version, RelativePath = "api/Products/{key}" },
                new { HttpMethod = Delete, version, RelativePath = "api/Suppliers/{key}" },
                new { HttpMethod = Delete, version, RelativePath = "api/Products/{key}/supplier/$ref" },
                new { HttpMethod = Delete, version, RelativePath = "api/Suppliers/{key}/Products/$ref?$id={$id}" },
            },
            options => options.ExcludingMissingMembers() );
    }

    [Fact]
    public void api_description_group_should_explore_model_bound_settings()
    {
        // arrange
        var configuration = new HttpConfiguration();
        var controllerTypeResolver = new ControllerTypeCollection(
            typeof( VersionedMetadataController ),
            typeof( Simulators.V1.BooksController ) );

        configuration.Services.Replace( typeof( IHttpControllerTypeResolver ), controllerTypeResolver );
        configuration.EnableDependencyInjection();
        configuration.AddApiVersioning();
        configuration.MapHttpAttributeRoutes();

        var apiVersion = new ApiVersion( 1.0 );
        var options = new ODataApiExplorerOptions( configuration );
        var apiExplorer = new ODataApiExplorer( configuration, options );

        options.AdHocModelBuilder.ModelConfigurations.Add( new ImplicitModelBoundSettingsConvention() );

        // act
        var descriptionGroup = apiExplorer.ApiDescriptions[apiVersion];
        var description = descriptionGroup.ApiDescriptions[0];

        // assert
        var parameter = description.ParameterDescriptions.Single( p => p.Name == "$filter" );

        parameter.Documentation.Should().EndWith( "author, published." );
    }
}