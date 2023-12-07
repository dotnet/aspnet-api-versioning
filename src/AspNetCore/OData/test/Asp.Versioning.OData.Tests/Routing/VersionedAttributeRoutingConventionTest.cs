// Copyright (c) .NET Foundation and contributors. All rights reserved.

//// Ignore Spelling: Dneutral

namespace Asp.Versioning.Routing;

using Asp.Versioning.OData;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.AspNetCore.OData.Routing.Parser;
using Microsoft.Extensions.Logging;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using System.Reflection;

public class VersionedAttributeRoutingConventionTest
{
    [Fact]
    public void applied_to_action_should_return_true_for_versionX2Dneutral_action()
    {
        // arrange
        var logger = Mock.Of<ILogger<AttributeRoutingConvention>>();
        var parser = Mock.Of<IODataPathTemplateParser>();
        var convention = new VersionedAttributeRoutingConvention( logger, parser );
        var action = new ActionModel(
            Mock.Of<MethodInfo>(),
            Array.Empty<object>() )
        {
            Selectors =
            {
                new()
                {
                    EndpointMetadata = { ApiVersionMetadata.Neutral },
                },
            },
        };
        var controller = new ControllerModel(
            typeof( ODataController ).GetTypeInfo(),
            Array.Empty<object>() )
        {
            Actions = { action },
        };

        var context = new ODataControllerActionContext(
            string.Empty,
            EdmCoreModel.Instance,
            controller )
        {
            Action = action,
        };

        // act
        var result = convention.AppliesToAction( context );

        // assert
        result.Should().BeFalse();
    }

    [Fact]
    public void applies_to_action_should_return_false_if_api_version_matches_edm_annotation()
    {
        // arrange
        var logger = Mock.Of<ILogger<AttributeRoutingConvention>>();
        var parser = Mock.Of<IODataPathTemplateParser>();
        var convention = new VersionedAttributeRoutingConvention( logger, parser );
        var action = new ActionModel(
            Mock.Of<MethodInfo>(),
            Array.Empty<object>() )
        {
            Selectors =
            {
                new()
                {
                    AttributeRouteModel = new() { Template = "/Tests" },
                    EndpointMetadata =
                    {
                        new ApiVersionMetadata(
                            new(ApiVersion.Default),
                            ApiVersionModel.Empty),
                    },
                },
            },
        };
        var controller = new ControllerModel(
            typeof( ODataController ).GetTypeInfo(),
            new object[] { new ODataAttributeRoutingAttribute() } )
        {
            Actions = { action },
        };
        var builder = new ODataConventionModelBuilder();

        builder.EntitySet<TestEntity>( "Tests" ).EntityType.HasKey( e => e.Id );

        var edm = builder.GetEdmModel();

        edm.SetAnnotationValue( edm, new ApiVersionAnnotation( ApiVersion.Default ) );

        var context = new ODataControllerActionContext( string.Empty, edm, controller )
        {
            Action = action,
            Options = new ODataOptions().AddRouteComponents( edm ),
        };

        // act
        var result = convention.AppliesToAction( context );

        // assert
        result.Should().BeFalse();
    }

    [Fact]
    public void applies_to_action_should_return_false_if_api_version_is_different_from_edm_annotation()
    {
        // arrange
        var logger = Mock.Of<ILogger<AttributeRoutingConvention>>();
        var parser = Mock.Of<IODataPathTemplateParser>();
        var convention = new VersionedAttributeRoutingConvention( logger, parser );
        var action = new ActionModel(
            Mock.Of<MethodInfo>(),
            Array.Empty<object>() )
        {
            Selectors =
            {
                new()
                {
                    AttributeRouteModel = new() { Template = "/Tests" },
                    EndpointMetadata =
                    {
                        new ApiVersionMetadata(
                            new( new ApiVersion( 1.0 ) ),
                            ApiVersionModel.Empty),
                    },
                },
            },
        };
        var controller = new ControllerModel(
            typeof( ODataController ).GetTypeInfo(),
            new object[] { new ODataAttributeRoutingAttribute() } )
        {
            Actions = { action },
        };
        var builder = new ODataConventionModelBuilder();

        builder.EntitySet<TestEntity>( "Tests" ).EntityType.HasKey( e => e.Id );

        var edm = builder.GetEdmModel();

        edm.SetAnnotationValue( edm, new ApiVersionAnnotation( new ApiVersion( 2.0 ) ) );

        var context = new ODataControllerActionContext( string.Empty, edm, controller )
        {
            Action = action,
            Options = new ODataOptions().AddRouteComponents( edm ),
        };

        // act
        var result = convention.AppliesToAction( context );

        // assert
        result.Should().BeFalse();
    }
}