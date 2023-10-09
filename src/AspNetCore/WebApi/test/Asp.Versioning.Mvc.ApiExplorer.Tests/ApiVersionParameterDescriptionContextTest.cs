// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApiExplorer;

using Asp.Versioning.Routing;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using static Asp.Versioning.ApiVersionParameterLocation;

public class ApiVersionParameterDescriptionContextTest
{
    [Fact]
    public void add_parameter_should_add_descriptor_for_query_parameter()
    {
        // arrange
        var version = new ApiVersion( 1, 0 );
        var description = NewApiDescription( version );
        var modelMetadata = new Mock<ModelMetadata>( ModelMetadataIdentity.ForType( typeof( string ) ) ).Object;
        var options = new ApiExplorerOptions()
        {
            DefaultApiVersion = version,
            ApiVersionParameterSource = new QueryStringApiVersionReader(),
        };
        var context = new ApiVersionParameterDescriptionContext( description, version, modelMetadata, options );

        // act
        context.AddParameter( "api-version", Query );

        // assert
        description.ParameterDescriptions.Single().Should().BeEquivalentTo(
            new
            {
                Name = "api-version",
                ModelMetadata = modelMetadata,
                Source = BindingSource.Query,
                DefaultValue = (object) "1.0",
                IsRequired = true,
                Type = typeof( string ),
            },
            o => o.ExcludingMissingMembers() );
    }

    [Fact]
    public void add_parameter_should_add_descriptor_for_header()
    {
        // arrange
        var version = new ApiVersion( 1, 0 );
        var description = NewApiDescription( version );
        var modelMetadata = new Mock<ModelMetadata>( ModelMetadataIdentity.ForType( typeof( string ) ) ).Object;
        var options = new ApiExplorerOptions()
        {
            DefaultApiVersion = version,
            ApiVersionParameterSource = new HeaderApiVersionReader(),
        };
        var context = new ApiVersionParameterDescriptionContext( description, version, modelMetadata, options );

        // act
        context.AddParameter( "api-version", Header );

        // assert
        description.ParameterDescriptions.Single().Should().BeEquivalentTo(
            new
            {
                Name = "api-version",
                ModelMetadata = modelMetadata,
                Source = BindingSource.Header,
                DefaultValue = (object) "1.0",
                IsRequired = true,
                Type = typeof( string ),
            },
            o => o.ExcludingMissingMembers() );
    }

    [Fact]
    public void add_parameter_should_add_descriptor_for_path()
    {
        // arrange
        var parameter = new ApiParameterDescription()
        {
            Name = "api-version",
            RouteInfo = new()
            {
                Constraints = new[] { new ApiVersionRouteConstraint() },
            },
            Source = BindingSource.Path,
        };
        var version = new ApiVersion( 1, 0 );
        var description = NewApiDescription( version, parameter );
        var modelMetadata = new Mock<ModelMetadata>( ModelMetadataIdentity.ForType( typeof( string ) ) ).Object;
        var options = new ApiExplorerOptions()
        {
            DefaultApiVersion = version,
            ApiVersionParameterSource = new UrlSegmentApiVersionReader(),
        };
        var context = new ApiVersionParameterDescriptionContext( description, version, modelMetadata, options );

        // act
        context.AddParameter( "api-version", Path );

        // assert
        description.ParameterDescriptions.Single().Should().BeEquivalentTo(
            new
            {
                Name = "api-version",
                ModelMetadata = modelMetadata,
                Source = BindingSource.Path,
                DefaultValue = (object) "1.0",
                IsRequired = true,
                RouteInfo = new ApiParameterRouteInfo()
                {
                    DefaultValue = "1.0",
                    IsOptional = false,
                    Constraints = parameter.RouteInfo.Constraints,
                },
                Type = typeof( string ),
            },
            o => o.ExcludingMissingMembers() );
    }

    [Fact]
    public void add_parameter_should_remove_other_descriptors_after_path_parameter_is_added()
    {
        // arrange
        var parameter = new ApiParameterDescription()
        {
            Name = "api-version",
            RouteInfo = new()
            {
                Constraints = new[] { new ApiVersionRouteConstraint() },
            },
            Source = BindingSource.Path,
        };
        var version = new ApiVersion( 1, 0 );
        var description = NewApiDescription( version, parameter );
        var modelMetadata = new Mock<ModelMetadata>( ModelMetadataIdentity.ForType( typeof( string ) ) );
        var options = new ApiExplorerOptions()
        {
            DefaultApiVersion = version,
            ApiVersionParameterSource = ApiVersionReader.Combine(
                new QueryStringApiVersionReader(),
                new UrlSegmentApiVersionReader() ),
        };
        var context = new ApiVersionParameterDescriptionContext( description, version, modelMetadata.Object, options );

        modelMetadata.SetupGet( m => m.DataTypeName ).Returns( nameof( ApiVersion ) );

        // act
        context.AddParameter( "api-version", Query );
        context.AddParameter( "api-version", Path );

        // assert
        description.ParameterDescriptions.Single().Should().BeEquivalentTo(
            new
            {
                Name = "api-version",
                ModelMetadata = modelMetadata.Object,
                Source = BindingSource.Path,
                DefaultValue = (object) "1.0",
                IsRequired = true,
                RouteInfo = new ApiParameterRouteInfo()
                {
                    DefaultValue = "1.0",
                    IsOptional = false,
                    Constraints = parameter.RouteInfo.Constraints,
                },
                Type = typeof( string ),
            },
            o => o.ExcludingMissingMembers() );
    }

    [Fact]
    public void add_parameter_should_not_add_query_parameter_after_path_parameter_has_been_added()
    {
        // arrange
        var parameter = new ApiParameterDescription()
        {
            Name = "api-version",
            RouteInfo = new()
            {
                Constraints = new[] { new ApiVersionRouteConstraint() },
            },
            Source = BindingSource.Path,
        };
        var version = new ApiVersion( 1, 0 );
        var description = NewApiDescription( version, parameter );
        var modelMetadata = new Mock<ModelMetadata>( ModelMetadataIdentity.ForType( typeof( string ) ) );
        var options = new ApiExplorerOptions()
        {
            DefaultApiVersion = version,
            ApiVersionParameterSource = ApiVersionReader.Combine(
                new QueryStringApiVersionReader(),
                new UrlSegmentApiVersionReader() ),
        };
        var context = new ApiVersionParameterDescriptionContext( description, version, modelMetadata.Object, options );

        modelMetadata.SetupGet( m => m.DataTypeName ).Returns( nameof( ApiVersion ) );

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
        const string Json = "application/json";
        var version = new ApiVersion( 1, 0 );
        var metadata = new ApiVersionMetadata( ApiVersionModel.Empty, new ApiVersionModel( version ) );
        var description = new ApiDescription()
        {
            ActionDescriptor = new() { EndpointMetadata = new[] { metadata } },
            SupportedRequestFormats = { new() { MediaType = Json } },
            SupportedResponseTypes = { new() { ApiResponseFormats = { new() { MediaType = Json } } } },
        };
        var modelMetadata = new Mock<ModelMetadata>( ModelMetadataIdentity.ForType( typeof( string ) ) ).Object;
        var options = new ApiExplorerOptions()
        {
            DefaultApiVersion = version,
            ApiVersionParameterSource = new MediaTypeApiVersionReader(),
        };
        var context = new ApiVersionParameterDescriptionContext( description, version, modelMetadata, options );

        // act
        context.AddParameter( "v", MediaTypeParameter );

        // assert
        description.SupportedRequestFormats
                   .Single()
                   .MediaType
                   .Should()
                   .Be( "application/json; v=1.0" );

        description.SupportedResponseTypes
                   .Single()
                   .ApiResponseFormats
                   .Single()
                   .MediaType
                   .Should()
                   .Be( "application/json; v=1.0" );
    }

    [Fact]
    public void add_parameter_should_add_optional_parameter_when_allowed()
    {
        // arrange
        var version = new ApiVersion( 2.0 );
        var description = NewApiDescription( version );
        var modelMetadata = new Mock<ModelMetadata>( ModelMetadataIdentity.ForType( typeof( string ) ) ).Object;
        var options = new ApiExplorerOptions()
        {
            DefaultApiVersion = ApiVersion.Default,
            ApiVersionParameterSource = new QueryStringApiVersionReader(),
            ApiVersionSelector = new ConstantApiVersionSelector( version ),
            AssumeDefaultVersionWhenUnspecified = true,
        };
        var context = new ApiVersionParameterDescriptionContext( description, version, modelMetadata, options );

        // act
        context.AddParameter( "api-version", Query );

        // assert
        description.ParameterDescriptions.Single().Should().BeEquivalentTo(
            new
            {
                Name = "api-version",
                ModelMetadata = modelMetadata,
                Source = BindingSource.Query,
                DefaultValue = (object) "2.0",
                IsRequired = false,
                Type = typeof( string ),
            },
            o => o.ExcludingMissingMembers() );
    }

    [Fact]
    public void add_parameter_should_make_parameters_optional_after_first_parameter()
    {
        // arrange
        var version = new ApiVersion( 1, 0 );
        var description = NewApiDescription( version );
        var modelMetadata = new Mock<ModelMetadata>( ModelMetadataIdentity.ForType( typeof( string ) ) ).Object;
        var options = new ApiExplorerOptions()
        {
            DefaultApiVersion = version,
            ApiVersionParameterSource = ApiVersionReader.Combine( new QueryStringApiVersionReader(), new HeaderApiVersionReader() ),
        };
        var context = new ApiVersionParameterDescriptionContext( description, version, modelMetadata, options );

        // act
        context.AddParameter( "api-version", Query );
        context.AddParameter( "api-version", Header );

        // assert
        description.ParameterDescriptions[0].IsRequired.Should().BeTrue();
        description.ParameterDescriptions[1].IsRequired.Should().BeFalse();
    }

    private static ApiDescription NewApiDescription( ApiVersion apiVersion, params ApiParameterDescription[] parameters )
    {
        var metadata = new ApiVersionMetadata( ApiVersionModel.Empty, new ApiVersionModel( apiVersion ) );
        var action = new ActionDescriptor() { EndpointMetadata = new[] { metadata } };
        var description = new ApiDescription() { ActionDescriptor = action };

        for ( var i = 0; i < parameters.Length; i++ )
        {
            description.ParameterDescriptions.Add( parameters[i] );
        }

        return description;
    }
}