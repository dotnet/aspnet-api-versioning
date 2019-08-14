namespace Microsoft.Web.Http.Description
{
    using FluentAssertions;
    using Microsoft.AspNet.OData;
    using System.Linq;
    using System.Net.Http;
    using System.Web.Http;
    using Xunit;
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
            var controllerTypes = configuration.Services.GetHttpControllerTypeResolver().GetControllerTypes( assembliesResolver );
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
            var controllerTypes = configuration.Services.GetHttpControllerTypeResolver().GetControllerTypes( assembliesResolver );
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
                        Version = apiVersion
                    },
                    new
                    {
                        ID = $"GET{relativePaths[1]}",
                        HttpMethod = Get,
                        RelativePath = relativePaths[1],
                        Version = apiVersion
                    },
                    new
                    {
                        ID = $"POST{relativePaths[2]}",
                        HttpMethod = Post,
                        RelativePath = relativePaths[2],
                        Version = apiVersion
                    },
                    new
                    {
                        ID = $"DELETE{relativePaths[3]}",
                        HttpMethod = Delete,
                        RelativePath = relativePaths[3],
                        Version = apiVersion
                    }
                },
                options => options.ExcludingMissingMembers() );
        }

        [Fact]
        public void api_description_group_should_explore_navigation_properties()
        {
            // arrange
            var Patch = new HttpMethod( "PATCH" );
            var Version = new ApiVersion( 3, 0 );
            var apiExplorer = new ODataApiExplorer( TestConfigurations.NewProductAndSupplierConfiguration() );
            var descriptionGroup = apiExplorer.ApiDescriptions[Version];

            // act
            var descriptions = descriptionGroup.ApiDescriptions;

            // assert
            descriptions.Should().BeEquivalentTo(
                new[]
                {
                    new { HttpMethod = Get, Version, RelativePath = "api/Products" },
                    new { HttpMethod = Get, Version, RelativePath = "api/Suppliers" },
                    new { HttpMethod = Get, Version, RelativePath = "api/Products/{key}" },
                    new { HttpMethod = Get, Version, RelativePath = "api/Suppliers/{key}" },
                    new { HttpMethod = Get, Version, RelativePath = "api/Products/{key}/Supplier" },
                    new { HttpMethod = Get, Version, RelativePath = "api/Suppliers/{key}/Products" },
                    new { HttpMethod = Get, Version, RelativePath = "api/Products/{key}/Supplier/$ref" },
                    new { HttpMethod = Put, Version, RelativePath = "api/Products/{key}" },
                    new { HttpMethod = Put, Version, RelativePath = "api/Suppliers/{key}" },
                    new { HttpMethod = Put, Version, RelativePath = "api/Products/{key}/Supplier/$ref" },
                    new { HttpMethod = Post, Version, RelativePath = "api/Products" },
                    new { HttpMethod = Post, Version, RelativePath = "api/Suppliers" },
                    new { HttpMethod = Post, Version, RelativePath = "api/Suppliers/{key}/Products/$ref" },
                    new { HttpMethod = Patch, Version, RelativePath = "api/Products/{key}" },
                    new { HttpMethod = Patch, Version, RelativePath = "api/Suppliers/{key}" },
                    new { HttpMethod = Delete, Version, RelativePath = "api/Products/{key}" },
                    new { HttpMethod = Delete, Version, RelativePath = "api/Suppliers/{key}" },
                    new { HttpMethod = Delete, Version, RelativePath = "api/Products/{key}/Supplier/$ref" },
                    new { HttpMethod = Delete, Version, RelativePath = "api/Suppliers/{key}/Products/$ref?$id={$id}" },
                },
                options => options.ExcludingMissingMembers() );
        }
    }
}