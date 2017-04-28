namespace Microsoft.Web.Http.Description
{
    using FluentAssertions;
    using Microsoft.Web.Http.Routing;
    using Models;
    using Moq;
    using Simulators;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Description;
    using System.Web.Http.Routing;
    using Xunit;
    using static InternalTypeExtensions;
    using static System.Net.Http.HttpMethod;
    using static System.Web.Http.Description.ApiParameterSource;

    public class VersionedApiExplorerTest
    {
        [Fact]
        public void api_descriptions_should_recognize_direct_routes()
        {
            // arrange
            var configuration = new HttpConfiguration();
            var routeTemplate = "api/values";
            var controllerDescriptor = new HttpControllerDescriptor( configuration, "ApiExplorerValues", typeof( ApiExplorerValuesController ) );
            var action = new ReflectedHttpActionDescriptor( controllerDescriptor, typeof( ApiExplorerValuesController ).GetMethod( "Get" ) );
            var actions = new ReflectedHttpActionDescriptor[] { action };

            configuration.Routes.Add( "Route", CreateDirectRoute( routeTemplate, actions ) );

            IApiExplorer apiExplorer = new VersionedApiExplorer( configuration );

            // act
            var descriptions = apiExplorer.ApiDescriptions;

            // assert
            descriptions.Single().Should().ShouldBeEquivalentTo(
                new { HttpMethod = Get, RelativePath = routeTemplate, ActionDescriptor = action },
                options => options.ExcludingMissingMembers() );
        }

        [Fact]
        public void api_descriptions_should_ignore_api_for_direct_route_action()
        {
            // arrange
            var configuration = new HttpConfiguration();
            var routeTemplate = "api/values";
            var controllerDescriptor = new HttpControllerDescriptor( configuration, "ApiExplorerValues", typeof( ApiExplorerValuesController ) );
            var actions = new ReflectedHttpActionDescriptor[]
            {
                new ReflectedHttpActionDescriptor( controllerDescriptor, typeof( ApiExplorerValuesController ).GetMethod( "Get" ) ),
                new ReflectedHttpActionDescriptor( controllerDescriptor, typeof( ApiExplorerValuesController ).GetMethod( "Post" ) ),
            };

            configuration.Routes.Add( "Route", CreateDirectRoute( routeTemplate, actions ) );

            IApiExplorer apiExplorer = new VersionedApiExplorer( configuration );

            // act
            var descriptions = apiExplorer.ApiDescriptions;

            // assert
            descriptions.Single().Should().ShouldBeEquivalentTo(
                new { HttpMethod = Get, RelativePath = routeTemplate },
                options => options.ExcludingMissingMembers() );
        }

        [Fact]
        public void api_descriptions_should_ignore_api_for_direct_route_controller()
        {
            // arrange
            var configuration = new HttpConfiguration();
            var routeTemplate = "api/values";
            var controllerDescriptor = new HttpControllerDescriptor( configuration, "IgnoreApiValues", typeof( IgnoreApiValuesController ) );
            var actions = new ReflectedHttpActionDescriptor[]
            {
                new ReflectedHttpActionDescriptor( controllerDescriptor, typeof( IgnoreApiValuesController ).GetMethod( "Get" ) ),
                new ReflectedHttpActionDescriptor( controllerDescriptor, typeof( IgnoreApiValuesController ).GetMethod( "Post" ) ),
            };

            configuration.Routes.Add( "Route", CreateDirectRoute( routeTemplate, actions ) );

            IApiExplorer apiExplorer = new VersionedApiExplorer( configuration );

            // act
            var descriptions = apiExplorer.ApiDescriptions;

            // assert
            descriptions.Should().BeEmpty();
        }

        [Fact]
        public void api_descriptions_should_recognize_composite_routes()
        {
            // arrange
            var configuration = new HttpConfiguration();
            var routeTemplate = "api/values";
            var controllerDescriptor = new HttpControllerDescriptor( configuration, "AttributeApiExplorerValues", typeof( AttributeApiExplorerValuesController ) );
            var action = new ReflectedHttpActionDescriptor( controllerDescriptor, typeof( AttributeApiExplorerValuesController ).GetMethod( "Action" ) );
            var actions = new ReflectedHttpActionDescriptor[] { action };
            var routeCollection = new List<IHttpRoute>() { CreateDirectRoute( routeTemplate, actions ) };
            var route = NewRouteCollectionRoute();

            route.EnsureInitialized( () => routeCollection );
            configuration.Routes.Add( "Route", route );

            IApiExplorer apiExplorer = new VersionedApiExplorer( configuration );

            // act
            var descriptions = apiExplorer.ApiDescriptions;

            // assert
            descriptions.Single().Should().ShouldBeEquivalentTo(
                new { HttpMethod = Get, RelativePath = routeTemplate, ActionDescriptor = action },
                options => options.ExcludingMissingMembers() );
        }

        [Fact]
        public void try_expand_uri_parameters_should_handle_duplicateX2C_caseX2Dinsensitive_route_values()
        {
            // arrange
            var parameterDescriptorMock = new Mock<HttpParameterDescriptor>();

            parameterDescriptorMock.SetupGet( p => p.ParameterType ).Returns( typeof( ClassWithId ) );

            var apiExplorer = new TestApiExplorer( new HttpConfiguration() );
            var descriptions = new List<ApiParameterDescription>()
            {
                new ApiParameterDescription() { Source = FromUri, Name = "id" },
                new ApiParameterDescription() { Source = FromUri, ParameterDescriptor = parameterDescriptorMock.Object },
            };

            // act
            var expanded = apiExplorer.TryExpandUriParameters( new HttpRoute(), New.ParsedRoute, descriptions, out var expandedRouteTemplate );

            // assert
            expanded.Should().BeTrue();
            expandedRouteTemplate.Should().Be( "?id={id}" );
        }

        [Theory]
        [InlineData( "?id={id}", typeof( int ), "id" )]
        [InlineData( "?id[0]={id[0]}&id[1]={id[1]}", typeof( int[] ), "id" )]
        [InlineData( "?id[0]={id[0]}&id[1]={id[1]}", typeof( string[] ), "id" )]
        [InlineData( "?id[0]={id[0]}&id[1]={id[1]}", typeof( IList<string> ), "id" )]
        [InlineData( "?id[0]={id[0]}&id[1]={id[1]}", typeof( List<string> ), "id" )]
        [InlineData( "?id[0]={id[0]}&id[1]={id[1]}", typeof( IEnumerable<string> ), "id" )]
        [InlineData( "?id[0]={id[0]}&id[1]={id[1]}", typeof( ICollection<int> ), "id" )]
        [InlineData( "?users[0].Name={users[0].Name}&users[0].Age={users[0].Age}&users[1].Name={users[1].Name}&users[1].Age={users[1].Age}", typeof( IEnumerable<User> ), "users" )]
        [InlineData( "?users[0].Name={users[0].Name}&users[0].Age={users[0].Age}&users[1].Name={users[1].Name}&users[1].Age={users[1].Age}", typeof( User[] ), "users" )]
        [InlineData( "?Foo={Foo}&Bar={Bar}", typeof( MutableObject ), "mutable" )]
        [InlineData( "?key={key}&value={value}", typeof( KeyValuePair<string, string> ), "pair" )]
        [InlineData( "?dict[0].key={dict[0].key}&dict[0].value={dict[0].value}&dict[1].key={dict[1].key}&dict[1].value={dict[1].value}", typeof( Dictionary<string, string> ), "dict" )]
        [InlineData( "?Foo={Foo}&Bar={Bar}&Capacity={Capacity}&Item={Item}", typeof( GenericMutableObject<string> ), "genericMutable" )]
        public void try_expand_uri_parameters_should_expand_parameter( string expectedPath, Type parameterType, string parameterName )
        {
            // arrange
            var apiExplorer = new TestApiExplorer( new HttpConfiguration() );
            var descriptions = new List<ApiParameterDescription>()
            {
                CreateApiParameterDescription( parameterType, parameterName )
            };

            // act
            var expanded = apiExplorer.TryExpandUriParameters( new HttpRoute(), New.ParsedRoute, descriptions, out var finalPath );

            // assert
            expanded.Should().BeTrue();
            finalPath.Should().Be( expectedPath );
        }

        [Fact]
        public void try_expand_uri_parameters_should_expand_composite_parameters()
        {
            // arrange
            var apiExplorer = new TestApiExplorer( new HttpConfiguration() );
            var descriptions = new List<ApiParameterDescription>()
            {
                CreateApiParameterDescription( typeof( int[] ), "id" ),
                CreateApiParameterDescription( typeof( ICollection<string> ), "property" ),
                CreateApiParameterDescription( typeof( string ), "name" ),
            };

            // act
            var expanded = apiExplorer.TryExpandUriParameters( new HttpRoute(), New.ParsedRoute, descriptions, out var finalPath );

            // assert
            expanded.Should().BeTrue();
            finalPath.Should().Be( "?id[0]={id[0]}&id[1]={id[1]}&property[0]={property[0]}&property[1]={property[1]}&name={name}" );
        }

        [Fact]
        public void api_descriptions_should_recognize_mixedX2Dcase_parameters()
        {
            // arrange
            var configuration = new HttpConfiguration();
            var routeTemplate = "api/values/{id}";
            var controllerDescriptor = new HttpControllerDescriptor( configuration, "ApiExplorerValues", typeof( DuplicatedIdController ) );
            var action = new ReflectedHttpActionDescriptor( controllerDescriptor, typeof( DuplicatedIdController ).GetMethod( "Get" ) );
            var actions = new ReflectedHttpActionDescriptor[] { action };

            configuration.Routes.Add( "Route", CreateDirectRoute( routeTemplate, actions ) );

            IApiExplorer apiExplorer = new VersionedApiExplorer( configuration );

            // act
            var descriptions = apiExplorer.ApiDescriptions;

            // assert
            descriptions.Single().Should().ShouldBeEquivalentTo(
               new { HttpMethod = Get, RelativePath = routeTemplate, ActionDescriptor = action },
               options => options.ExcludingMissingMembers() );
        }

        [Theory]
        [ClassData( typeof( TestConfigurations ) )]
        public void api_descriptions_should_collate_expected_versions( HttpConfiguration configuration )
        {
            // arrange
            var apiExplorer = new VersionedApiExplorer( configuration );

            // act
            var descriptions = apiExplorer.ApiDescriptions;

            // assert
            descriptions.ApiVersions.Should().Equal(
                new ApiVersion( 1, 0 ),
                new ApiVersion( 2, 0 ),
                new ApiVersion( 3, 0, "beta" ),
                new ApiVersion( 3, 0 ),
                new ApiVersion( 4, 0 ) );
        }

        [Theory]
        [ClassData( typeof( TestConfigurations ) )]
        public void api_descriptions_should_group_versioned_controllers( HttpConfiguration configuration )
        {
            // arrange
            var assembliesResolver = configuration.Services.GetAssembliesResolver();
            var controllerTypes = configuration.Services.GetHttpControllerTypeResolver().GetControllerTypes( assembliesResolver );
            var apiExplorer = new VersionedApiExplorer( configuration );

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
            var apiExplorer = new VersionedApiExplorer( configuration );

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
        public void api_description_group_should_explore_v1_actions( HttpConfiguration configuration )
        {
            // arrange
            var apiExplorer = new VersionedApiExplorer( configuration );
            var apiVersion = new ApiVersion( 1, 0 );
            var descriptionGroup = apiExplorer.ApiDescriptions[apiVersion];

            // act
            var description = descriptionGroup.ApiDescriptions.Single();
            var relativePath = description.RelativePath;

            // assert
            description.ShouldBeEquivalentTo(
                new
                {
                    ID = $"GET{relativePath}",
                    HttpMethod = Get,
                    RelativePath = relativePath,
                    Version = apiVersion
                },
                options => options.ExcludingMissingMembers() );
        }

        [Theory]
        [ClassData( typeof( TestConfigurations ) )]
        public void api_description_group_should_explore_v2_actions( HttpConfiguration configuration )
        {
            // arrange
            var apiExplorer = new VersionedApiExplorer( configuration );
            var apiVersion = new ApiVersion( 2, 0 );
            var descriptionGroup = apiExplorer.ApiDescriptions[apiVersion];

            // act
            var descriptions = descriptionGroup.ApiDescriptions;
            var relativePaths = descriptions.Select( d => d.RelativePath ).ToArray();

            // assert
            descriptions.ShouldBeEquivalentTo(
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
                    }
                },
                options => options.ExcludingMissingMembers() );
        }

        [Theory]
        [ClassData( typeof( TestConfigurations ) )]
        public void api_description_group_should_explore_v3_actions( HttpConfiguration configuration )
        {
            // arrange
            var apiExplorer = new VersionedApiExplorer( configuration );
            var apiVersion = new ApiVersion( 3, 0 );
            var descriptionGroup = apiExplorer.ApiDescriptions[apiVersion];

            // act
            var descriptions = descriptionGroup.ApiDescriptions;
            var relativePaths = descriptions.Select( d => d.RelativePath ).ToArray();

            // assert
            descriptions.ShouldBeEquivalentTo(
                new[]
                {
                    new
                    {
                        ID = $"GET{relativePaths[0]}",
                        HttpMethod = Get,
                        RelativePath = relativePaths[0],
                        Version = apiVersion,
                        ActionDescriptor = new { ActionName = "GetV3" }
                    },
                    new
                    {
                        ID = $"GET{relativePaths[1]}",
                        HttpMethod = Get,
                        RelativePath = relativePaths[1],
                        Version = apiVersion,
                        ActionDescriptor = new { ActionName = "Get" }
                    },
                    new
                    {
                        ID = $"POST{relativePaths[2]}",
                        HttpMethod = Post,
                        RelativePath = relativePaths[2],
                        Version = apiVersion,
                        ActionDescriptor = new { ActionName = "Post" }
                    }
                },
                options => options.ExcludingMissingMembers() );
        }

        [Theory]
        [ClassData( typeof( TestConfigurations ) )]
        public void api_description_group_should_explore_v3_beta_actions( HttpConfiguration configuration )
        {
            // arrange
            var apiExplorer = new VersionedApiExplorer( configuration );
            var apiVersion = new ApiVersion( 3, 0, "beta" );
            var descriptionGroup = apiExplorer.ApiDescriptions[apiVersion];

            // act
            var descriptions = descriptionGroup.ApiDescriptions;
            var relativePaths = descriptions.Select( d => d.RelativePath ).ToArray();

            // assert
            descriptionGroup.IsDeprecated.Should().BeTrue();
            descriptions.ShouldBeEquivalentTo(
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
                    }
                },
                options => options.ExcludingMissingMembers() );
        }

        [Theory]
        [ClassData( typeof( TestConfigurations ) )]
        public void api_description_group_should_explore_v4_actions( HttpConfiguration configuration )
        {
            // arrange
            var apiExplorer = new VersionedApiExplorer( configuration );
            var apiVersion = new ApiVersion( 4, 0 );
            var descriptionGroup = apiExplorer.ApiDescriptions[apiVersion];

            // act
            var descriptions = descriptionGroup.ApiDescriptions;
            var relativePaths = descriptions.Select( d => d.RelativePath ).ToArray();

            // assert
            descriptions.ShouldBeEquivalentTo(
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

        static IHttpRoute CreateDirectRoute( string template, IReadOnlyCollection<ReflectedHttpActionDescriptor> actions )
        {
            var builder = NewDirectRouteBuilder( actions, targetIsAction: true );
            builder.Template = template;
            return builder.Build().Route;
        }

        static ApiParameterDescription CreateApiParameterDescription( Type type, string name )
        {
            var parameterDescriptorMock = new Mock<HttpParameterDescriptor>();

            parameterDescriptorMock.SetupGet( p => p.ParameterName ).Returns( name );
            parameterDescriptorMock.SetupGet( p => p.ParameterType ).Returns( type );

            return new ApiParameterDescription()
            {
                Source = FromUri,
                ParameterDescriptor = parameterDescriptorMock.Object,
                Name = name
            };
        }

        sealed class TestApiExplorer : VersionedApiExplorer
        {
            public TestApiExplorer( HttpConfiguration configuration ) : base( configuration ) { }

            new public bool TryExpandUriParameters(
                IHttpRoute route,
                IParsedRoute parsedRoute,
                ICollection<ApiParameterDescription> parameterDescriptions,
                out string expandedRouteTemplate ) =>
                base.TryExpandUriParameters( route, parsedRoute, parameterDescriptions, out expandedRouteTemplate );
        }

        static class New
        {
            internal static IParsedRoute ParsedRoute => new RouteParser().CreateNew();
        }
    }
}