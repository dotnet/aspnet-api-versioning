﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.

//// Ignore Spelling: Dcase
//// Ignore Spelling: Dinsensitive

namespace Asp.Versioning.ApiExplorer;

using Asp.Versioning.Models;
using Asp.Versioning.Routing;
using Asp.Versioning.Simulators;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using System.Web.Http.Routing;
using static Asp.Versioning.Description.InternalTypeExtensions;
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
        var metadata = new ApiVersionMetadata( ApiVersionModel.Empty, new ApiVersionModel( new ApiVersion( 1, 0 ) ) );
        var controller = new HttpControllerDescriptor( configuration, "ApiExplorerValues", typeof( ApiExplorerValuesController ) );
        var action = new ReflectedHttpActionDescriptor( controller, typeof( ApiExplorerValuesController ).GetMethod( "Get" ) )
        {
            Properties = { [typeof( ApiVersionMetadata )] = metadata },
        };
        var actions = new ReflectedHttpActionDescriptor[] { action };

        configuration.Routes.Add( "Route", CreateDirectRoute( routeTemplate, actions ) );

        IApiExplorer apiExplorer = new VersionedApiExplorer( configuration );

        // act
        var descriptions = apiExplorer.ApiDescriptions;

        // assert
        descriptions.Single().Should().BeEquivalentTo(
            new { HttpMethod = Get, RelativePath = routeTemplate, ActionDescriptor = action },
            options => options.ExcludingMissingMembers() );
    }

    [Fact]
    public void api_descriptions_should_ignore_api_for_direct_route_action()
    {
        // arrange
        var configuration = new HttpConfiguration();
        var routeTemplate = "api/values";
        var metadata = new ApiVersionMetadata( ApiVersionModel.Empty, new ApiVersionModel( new ApiVersion( 1, 0 ) ) );
        var controller = new HttpControllerDescriptor( configuration, "ApiExplorerValues", typeof( ApiExplorerValuesController ) );
        var actions = new ReflectedHttpActionDescriptor[]
        {
                new( controller, typeof( ApiExplorerValuesController ).GetMethod( "Get" ) )
                {
                    Properties = { [typeof( ApiVersionMetadata )] = metadata },
                },
                new( controller, typeof( ApiExplorerValuesController ).GetMethod( "Post" ) )
                {
                    Properties = { [typeof( ApiVersionMetadata )] = metadata },
                },
        };

        configuration.Routes.Add( "Route", CreateDirectRoute( routeTemplate, actions ) );

        IApiExplorer apiExplorer = new VersionedApiExplorer( configuration );

        // act
        var descriptions = apiExplorer.ApiDescriptions;

        // assert
        descriptions.Single().Should().BeEquivalentTo(
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
                new( controllerDescriptor, typeof( IgnoreApiValuesController ).GetMethod( "Get" ) ),
                new( controllerDescriptor, typeof( IgnoreApiValuesController ).GetMethod( "Post" ) ),
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
        var metadata = new ApiVersionMetadata( ApiVersionModel.Empty, new ApiVersionModel( new ApiVersion( 1, 0 ) ) );
        var controllerDescriptor = new HttpControllerDescriptor( configuration, "AttributeApiExplorerValues", typeof( AttributeApiExplorerValuesController ) );
        var action = new ReflectedHttpActionDescriptor( controllerDescriptor, typeof( AttributeApiExplorerValuesController ).GetMethod( "Action" ) )
        {
            Properties = { [typeof( ApiVersionMetadata )] = metadata },
        };
        var actions = new ReflectedHttpActionDescriptor[] { action };
        var routeCollection = new List<IHttpRoute>() { CreateDirectRoute( routeTemplate, actions ) };
        var route = NewRouteCollectionRoute();

        route.EnsureInitialized( () => routeCollection );
        configuration.Routes.Add( "Route", route );

        IApiExplorer apiExplorer = new VersionedApiExplorer( configuration );

        // act
        var descriptions = apiExplorer.ApiDescriptions;

        // assert
        descriptions.Single().Should().BeEquivalentTo(
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
            new() { Source = FromUri, Name = "id" },
            new() { Source = FromUri, ParameterDescriptor = parameterDescriptorMock.Object },
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
                CreateApiParameterDescription( parameterType, parameterName ),
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
        var routeTemplate = "api/values/{Id}";
        var metadata = new ApiVersionMetadata( ApiVersionModel.Empty, new ApiVersionModel( new ApiVersion( 1, 0 ) ) );
        var controllerDescriptor = new HttpControllerDescriptor( configuration, "ApiExplorerValues", typeof( DuplicatedIdController ) );
        var action = new ReflectedHttpActionDescriptor( controllerDescriptor, typeof( DuplicatedIdController ).GetMethod( "Get" ) )
        {
            Properties = { [typeof( ApiVersionMetadata )] = metadata },
        };
        var actions = new ReflectedHttpActionDescriptor[] { action };

        configuration.Routes.Add( "Route", CreateDirectRoute( routeTemplate, actions ) );

        IApiExplorer apiExplorer = new VersionedApiExplorer( configuration );

        // act
        var descriptions = apiExplorer.ApiDescriptions;

        // assert
        descriptions.Single().Should().BeEquivalentTo(
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

        // assert
        description.Should().BeEquivalentTo(
            new
            {
                ID = "GETValues",
                HttpMethod = Get,
                RelativePath = "Values",
                Version = apiVersion,
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

        // assert
        descriptions.Should().BeEquivalentTo(
            new[]
            {
                    new
                    {
                        ID = "GETValues",
                        HttpMethod = Get,
                        RelativePath = "Values",
                        Version = apiVersion,
                    },
                    new
                    {
                        ID = "GETValues/{id}",
                        HttpMethod = Get,
                        RelativePath = "Values/{id}",
                        Version = apiVersion,
                    },
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

        // assert
        descriptions.Should().BeEquivalentTo(
            new[]
            {
                    new
                    {
                        ID = "GETValues",
                        HttpMethod = Get,
                        RelativePath = "Values",
                        Version = apiVersion,
                        ActionDescriptor = new { ActionName = "GetV3" },
                    },
                    new
                    {
                        ID = "GETValues/{id}",
                        HttpMethod = Get,
                        RelativePath = "Values/{id}",
                        Version = apiVersion,
                        ActionDescriptor = new { ActionName = "Get" },
                    },
                    new
                    {
                        ID = "POSTValues",
                        HttpMethod = Post,
                        RelativePath = "Values",
                        Version = apiVersion,
                        ActionDescriptor = new { ActionName = "Post" },
                    },
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

        // assert
        descriptionGroup.IsDeprecated.Should().BeTrue();
        descriptions.Should().BeEquivalentTo(
            new[]
            {
                    new
                    {
                        ID = "GETValues",
                        HttpMethod = Get,
                        RelativePath = "Values",
                        Version = apiVersion,
                    },
                    new
                    {
                        ID = "GETValues/{id}",
                        HttpMethod = Get,
                        RelativePath = "Values/{id}",
                        Version = apiVersion,
                    },
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

        // assert
        descriptions.Should().BeEquivalentTo(
            new[]
            {
                    new
                    {
                        ID = "GETValues",
                        HttpMethod = Get,
                        RelativePath = "Values",
                        Version = apiVersion,
                    },
                    new
                    {
                        ID = "GETValues/{id}",
                        HttpMethod = Get,
                        RelativePath = "Values/{id}",
                        Version = apiVersion,
                    },
                    new
                    {
                        ID = "POSTValues",
                        HttpMethod = Post,
                        RelativePath = "Values",
                        Version = apiVersion,
                    },
                    new
                    {
                        ID = "DELETEValues/{id}",
                        HttpMethod = Delete,
                        RelativePath = "Values/{id}",
                        Version = apiVersion,
                    },
            },
            options => options.ExcludingMissingMembers() );
    }

    private static IHttpRoute CreateDirectRoute( string template, IReadOnlyCollection<ReflectedHttpActionDescriptor> actions )
    {
        var builder = NewDirectRouteBuilder( actions, targetIsAction: true );
        builder.Template = template;
        return builder.Build().Route;
    }

    private static ApiParameterDescription CreateApiParameterDescription( Type type, string name )
    {
        var parameterDescriptorMock = new Mock<HttpParameterDescriptor>();

        parameterDescriptorMock.SetupGet( p => p.ParameterName ).Returns( name );
        parameterDescriptorMock.SetupGet( p => p.ParameterType ).Returns( type );

        return new ApiParameterDescription()
        {
            Source = FromUri,
            ParameterDescriptor = parameterDescriptorMock.Object,
            Name = name,
        };
    }

    private sealed class TestApiExplorer : VersionedApiExplorer
    {
        public TestApiExplorer( HttpConfiguration configuration ) : base( configuration ) { }

        public new bool TryExpandUriParameters(
            IHttpRoute route,
            IParsedRoute parsedRoute,
            ICollection<ApiParameterDescription> parameterDescriptions,
            out string expandedRouteTemplate ) =>
            base.TryExpandUriParameters( route, parsedRoute, parameterDescriptions, out expandedRouteTemplate );
    }

    private static class New
    {
        internal static IParsedRoute ParsedRoute => new RouteParser().CreateNew();
    }
}