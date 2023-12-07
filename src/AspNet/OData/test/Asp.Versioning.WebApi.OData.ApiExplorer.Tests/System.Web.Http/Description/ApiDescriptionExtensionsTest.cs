// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace System.Web.Http.Description;

using Asp.Versioning;
using Asp.Versioning.Description;
using Asp.Versioning.Simulators.Models;
using Microsoft.AspNet.OData.Builder;
using Microsoft.OData.Edm;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Controllers;

public class ApiDescriptionExtensionsTest
{
    [Fact]
    public void edm_model_should_be_retrieved_from_properties()
    {
        // arrange
        var model = CreateEdmModel();
        var apiDescription = CreateApiDescription( model );

        // act
        var result = apiDescription.EdmModel();

        // assert
        result.Should().BeSameAs( model );
    }

    [Fact]
    public void entity_set_should_be_retrieved_from_properties()
    {
        // arrange
        var model = CreateEdmModel();
        var entitySet = model.EntityContainer.FindEntitySet( "Orders" );
        var apiDescription = CreateApiDescription( model );

        // act
        var result = apiDescription.EntitySet();

        // assert
        result.Should().BeSameAs( entitySet );
    }

    [Fact]
    public void entity_type_should_be_retrieved_from_properties()
    {
        // arrange
        var model = CreateEdmModel();
        var entityType = model.EntityContainer.FindEntitySet( "Orders" ).EntityType();
        var apiDescription = CreateApiDescription( model );

        // act
        var result = apiDescription.EntityType();

        // assert
        result.Should().BeSameAs( entityType );
    }

    private static IEdmModel CreateEdmModel()
    {
        var builder = new ODataConventionModelBuilder();
        builder.EntitySet<Order>( "Orders" );
        return builder.GetEdmModel();
    }

    private static VersionedApiDescription CreateApiDescription( IEdmModel model )
    {
        var configuration = new HttpConfiguration();
        var controllerType = typeof( Asp.Versioning.Simulators.V1.OrdersController );
        var actionMethod = controllerType.GetRuntimeMethod( "Get", [typeof( int )] );
        var controllerDescriptor = new HttpControllerDescriptor( configuration, "Orders", controllerType );
        var actionDescriptor = new ReflectedHttpActionDescriptor( controllerDescriptor, actionMethod );
        var apiDescription = new VersionedApiDescription()
        {
            ActionDescriptor = actionDescriptor,
            ApiVersion = new ApiVersion( 1, 0 ),
            Properties = { [typeof( IEdmModel )] = model },
        };

        return apiDescription;
    }
}