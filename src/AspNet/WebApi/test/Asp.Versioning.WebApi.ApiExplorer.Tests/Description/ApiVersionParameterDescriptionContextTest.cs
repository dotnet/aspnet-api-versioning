// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Description;

using Asp.Versioning.ApiExplorer;
using Asp.Versioning.Routing;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using System.Web.Http.Routing;
using static Asp.Versioning.ApiVersionParameterLocation;
using static System.Web.Http.Description.ApiParameterSource;

public class ApiVersionParameterDescriptionContextTest
{
    [Fact]
    public void add_parameter_should_add_descriptor_for_query_parameter()
    {
        // arrange
        var configuration = new HttpConfiguration();
        var action = NewActionDescriptor();
        var description = new ApiDescription() { ActionDescriptor = action };
        var version = new ApiVersion( 1, 0 );
        var options = new ApiExplorerOptions( configuration );
        var context = new ApiVersionParameterDescriptionContext( description, version, options );

        action.Configuration = configuration;

        // act
        context.AddParameter( "api-version", Query );

        // assert
        description.ParameterDescriptions.Single().Should().BeEquivalentTo(
            new
            {
                Name = "api-version",
                Documentation = options.DefaultApiVersionParameterDescription,
                Source = FromUri,
                ParameterDescriptor = new
                {
                    ParameterName = "api-version",
                    DefaultValue = "1.0",
                    IsOptional = false,
                    Configuration = configuration,
                    ActionDescriptor = action,
                },
            },
            o => o.ExcludingMissingMembers() );
    }

    [Fact]
    public void add_parameter_should_add_descriptor_for_header()
    {
        // arrange
        var configuration = new HttpConfiguration();
        var action = NewActionDescriptor();
        var description = new ApiDescription() { ActionDescriptor = action };
        var version = new ApiVersion( 1, 0 );
        var options = new ApiExplorerOptions( configuration );
        var context = new ApiVersionParameterDescriptionContext( description, version, options );

        action.Configuration = configuration;

        // act
        context.AddParameter( "api-version", Header );

        // assert
        description.ParameterDescriptions.Single().Should().BeEquivalentTo(
            new
            {
                Name = "api-version",
                Documentation = options.DefaultApiVersionParameterDescription,
                Source = Unknown,
                ParameterDescriptor = new
                {
                    ParameterName = "api-version",
                    DefaultValue = "1.0",
                    IsOptional = false,
                    Configuration = configuration,
                    ActionDescriptor = action,
                },
            },
            o => o.ExcludingMissingMembers() );
    }

    [Fact]
    public void add_parameter_should_add_descriptor_for_path()
    {
        // arrange
        var configuration = new HttpConfiguration();
        var action = NewActionDescriptor();
        var route = new HttpRoute() { Constraints = { ["api-version"] = new ApiVersionRouteConstraint() } };
        var description = new ApiDescription()
        {
            ActionDescriptor = action,
            Route = route,
        };
        var version = new ApiVersion( 1, 0 );
        var options = new ApiExplorerOptions( configuration );
        var context = new ApiVersionParameterDescriptionContext( description, version, options );

        action.Configuration = configuration;
        description.ParameterDescriptions.Add( new ApiParameterDescription() { Name = "api-version", Source = FromUri } );

        // act
        context.AddParameter( "api-version", Path );

        // assert
        description.ParameterDescriptions.Single().Should().BeEquivalentTo(
            new
            {
                Name = "api-version",
                Documentation = options.DefaultApiVersionParameterDescription,
                Source = FromUri,
                ParameterDescriptor = new
                {
                    ParameterName = "api-version",
                    DefaultValue = "1.0",
                    IsOptional = false,
                    Configuration = configuration,
                    ActionDescriptor = action,
                },
            },
            o => o.ExcludingMissingMembers() );
    }

    [Fact]
    public void add_parameter_should_remove_other_descriptors_after_path_parameter_is_added()
    {
        // arrange
        var configuration = new HttpConfiguration();
        var action = NewActionDescriptor();
        var route = new HttpRoute() { Constraints = { ["api-version"] = new ApiVersionRouteConstraint() } };
        var description = new ApiDescription()
        {
            ActionDescriptor = action,
            Route = route,
        };
        var version = new ApiVersion( 1, 0 );
        var options = new ApiExplorerOptions( configuration );
        var context = new ApiVersionParameterDescriptionContext( description, version, options );

        action.Configuration = configuration;
        description.ParameterDescriptions.Add( new ApiParameterDescription() { Name = "api-version", Source = FromUri } );

        // act
        context.AddParameter( "api-version", Query );
        context.AddParameter( "api-version", Path );

        // assert
        description.ParameterDescriptions.Should().HaveCount( 1 );
    }

    [Fact]
    public void add_parameter_should_not_add_query_parameter_after_path_parameter_has_been_added()
    {
        // arrange
        var configuration = new HttpConfiguration();
        var action = NewActionDescriptor();
        var route = new HttpRoute() { Constraints = { ["api-version"] = new ApiVersionRouteConstraint() } };
        var description = new ApiDescription()
        {
            ActionDescriptor = action,
            Route = route,
        };
        var version = new ApiVersion( 1, 0 );
        var options = new ApiExplorerOptions( configuration );
        var context = new ApiVersionParameterDescriptionContext( description, version, options );

        action.Configuration = configuration;
        description.ParameterDescriptions.Add( new ApiParameterDescription() { Name = "api-version", Source = FromUri } );

        // act
        context.AddParameter( "api-version", Path );
        context.AddParameter( "api-version", Query );

        // assert
        description.ParameterDescriptions.Should().HaveCount( 1 );
    }

    [Fact]
    public void add_parameter_should_add_descriptor_for_media_type_parameter()
    {
        // arrange
        var configuration = new HttpConfiguration();
        var action = NewActionDescriptor();
        var json = new JsonMediaTypeFormatter();
        var formUrlEncoded = new FormUrlEncodedMediaTypeFormatter();

        configuration.Formatters.Clear();
        configuration.Formatters.Add( json );
        configuration.Formatters.Add( formUrlEncoded );
        action.Configuration = configuration;

        var description = new ApiDescription()
        {
            ActionDescriptor = action,
            SupportedRequestBodyFormatters = { json, formUrlEncoded },
        };
        var version = new ApiVersion( 1, 0 );
        var options = new ApiExplorerOptions( configuration );
        var context = new ApiVersionParameterDescriptionContext( description, version, options );

        // act
        context.AddParameter( "v", MediaTypeParameter );

        // assert
        var formatter = description.SupportedRequestBodyFormatters[0];

        foreach ( var mediaType in formatter.SupportedMediaTypes )
        {
            mediaType.Parameters.Single().Should().Be( new NameValueHeaderValue( "v", "1.0" ) );
        }

        formatter.Should().NotBeSameAs( json );
        formatter = description.SupportedRequestBodyFormatters[1];

        foreach ( var mediaType in formatter.SupportedMediaTypes )
        {
            mediaType.Parameters.Single().Should().Be( new NameValueHeaderValue( "v", "1.0" ) );
        }

        formatter.Should().NotBeSameAs( formUrlEncoded );
    }

    [Fact]
    public void add_parameter_should_add_optional_parameter_when_allowed()
    {
        // arrange
        var configuration = new HttpConfiguration();
        var action = NewActionDescriptor();
        var description = new ApiDescription() { ActionDescriptor = action };
        var version = new ApiVersion( 2.0 );
        var options = new ApiExplorerOptions( configuration )
        {
            ApiVersionSelector = new ConstantApiVersionSelector( version ),
        };

        action.Configuration = configuration;
        configuration.AddApiVersioning(
            options =>
            {
                options.DefaultApiVersion = ApiVersion.Default;
                options.AssumeDefaultVersionWhenUnspecified = true;
            } );

        var context = new ApiVersionParameterDescriptionContext( description, version, options );

        // act
        context.AddParameter( "api-version", Query );

        // assert
        description.ParameterDescriptions.Single().Should().BeEquivalentTo(
            new
            {
                Name = "api-version",
                Documentation = options.DefaultApiVersionParameterDescription,
                Source = FromUri,
                ParameterDescriptor = new
                {
                    ParameterName = "api-version",
                    DefaultValue = "2.0",
                    IsOptional = true,
                    Configuration = configuration,
                    ActionDescriptor = action,
                },
            },
            o => o.ExcludingMissingMembers() );
    }

    [Fact]
    public void add_parameter_should_make_parameters_optional_after_first_parameter()
    {
        // arrange
        var configuration = new HttpConfiguration();
        var action = NewActionDescriptor();
        var description = new ApiDescription() { ActionDescriptor = action };
        var version = new ApiVersion( 1, 0 );
        var options = new ApiExplorerOptions( configuration );
        var context = new ApiVersionParameterDescriptionContext( description, version, options );

        action.Configuration = configuration;

        // act
        context.AddParameter( "api-version", Query );
        context.AddParameter( "api-version", Header );

        // assert
        description.ParameterDescriptions[0].ParameterDescriptor.IsOptional.Should().BeFalse();
        description.ParameterDescriptions[1].ParameterDescriptor.IsOptional.Should().BeTrue();
    }

    private static HttpActionDescriptor NewActionDescriptor()
    {
        var action = new Mock<HttpActionDescriptor>() { CallBase = true }.Object;
        var controller = new Mock<HttpControllerDescriptor>() { CallBase = true };

        controller.Setup( c => c.GetCustomAttributes<IApiVersionProvider>( It.IsAny<bool>() ) ).Returns( [] );
        controller.Setup( c => c.GetCustomAttributes<IApiVersionNeutral>( It.IsAny<bool>() ) ).Returns( [] );
        controller.Setup( c => c.GetFilters() ).Returns( [] );
        action.ControllerDescriptor = controller.Object;

        return action;
    }
}