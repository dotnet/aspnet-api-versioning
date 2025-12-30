// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Description;

using Asp.Versioning.Controllers;
using Asp.Versioning.Conventions;
using Asp.Versioning.OData;
using Asp.Versioning.Simulators.Configuration;
using Asp.Versioning.Simulators.Models;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using static Asp.Versioning.Description.TestConfigurations;

public class TestConfigurations : TheoryData<EdmKind>
{
    public enum EdmKind
    {
        /// <summary>
        /// Indicates the Orders EDM.
        /// </summary>
        Orders,

        /// <summary>
        /// Indicates the People EDM.
        /// </summary>
        People,
    }

    public TestConfigurations()
    {
        Add( EdmKind.Orders );
        Add( EdmKind.People );
    }

    public static HttpConfiguration Get( EdmKind kind ) =>
        kind switch
        {
            EdmKind.Orders => NewOrdersConfiguration(),
            EdmKind.People => NewPeopleConfiguration(),
            _ => throw new ArgumentOutOfRangeException( nameof( kind ) ),
        };

    public static HttpConfiguration NewOrdersConfiguration()
    {
        var configuration = new HttpConfiguration();
        var controllerTypeResolver = new ControllerTypeCollection(
            typeof( VersionedMetadataController ),
            typeof( Simulators.V1.OrdersController ),
            typeof( Simulators.V2.OrdersController ),
            typeof( Simulators.V3.OrdersController ) );

        configuration.Services.Replace( typeof( IHttpControllerTypeResolver ), controllerTypeResolver );
        configuration.AddApiVersioning(
            options =>
            {
                options.Conventions.Controller<Simulators.V1.OrdersController>()
                                   .HasApiVersion( 1, 0 )
                                   .HasDeprecatedApiVersion( 0, 9 )
                                   .Action( c => c.Post( default ) ).MapToApiVersion( 1, 0 );
                options.Conventions.Controller<Simulators.V2.OrdersController>()
                                   .HasApiVersion( 2, 0 );
                options.Conventions.Controller<Simulators.V3.OrdersController>()
                                   .HasApiVersion( 3, 0 )
                                   .AdvertisesApiVersion( 4, 0 );
            } );
        var builder = new VersionedODataModelBuilder( configuration )
        {
            ModelConfigurations = { new OrderModelConfiguration() },
        };
        var models = builder.GetEdmModels();

        configuration.MapVersionedODataRoute( "odata", "api", models );

        return configuration;
    }

    public static HttpConfiguration NewPeopleConfiguration()
    {
        var configuration = new HttpConfiguration();
        var controllerTypeResolver = new ControllerTypeCollection(
            typeof( VersionedMetadataController ),
            typeof( Simulators.V1.PeopleController ),
            typeof( Simulators.V2.PeopleController ),
            typeof( Simulators.V3.PeopleController ) );

        configuration.Services.Replace( typeof( IHttpControllerTypeResolver ), controllerTypeResolver );
        configuration.AddApiVersioning();

        var builder = new VersionedODataModelBuilder( configuration )
        {
            ModelConfigurations = { new PersonModelConfiguration() },
        };
        var models = builder.GetEdmModels();

        configuration.MapVersionedODataRoute( "odata", "api/v{apiVersion}", models );

        return configuration;
    }

    public static HttpConfiguration NewProductAndSupplierConfiguration()
    {
        var configuration = new HttpConfiguration();
        var controllerTypeResolver = new ControllerTypeCollection(
            typeof( VersionedMetadataController ),
            typeof( Simulators.V3.ProductsController ),
            typeof( Simulators.V3.SuppliersController ) );

        configuration.Services.Replace( typeof( IHttpControllerTypeResolver ), controllerTypeResolver );
        configuration.AddApiVersioning();

        var builder = new VersionedODataModelBuilder( configuration )
        {
            DefaultModelConfiguration = ( b, v, r ) =>
            {
                b.EntitySet<Product>( "Products" ).EntityType.HasKey( p => p.Id );
                b.EntitySet<Supplier>( "Suppliers" ).EntityType.HasKey( s => s.Id );
            },
        };
        var models = builder.GetEdmModels();

        configuration.MapVersionedODataRoute( "odata", "api", models );

        return configuration;
    }
}