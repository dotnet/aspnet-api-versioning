// Copyright (c) .NET Foundation and contributors. All rights reserved.


namespace Asp.Versioning.Conventions;

using System.Collections.ObjectModel;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Controllers;
using static Asp.Versioning.ApiVersionMapping;

public partial class ControllerApiVersionConventionBuilderTest
{
    [Fact]
    public void apply_to_should_assign_conventions_to_controller()
    {
        // arrange
        var configuration = new HttpConfiguration();
        var mock = new Mock<HttpControllerDescriptor>() { CallBase = true };
        var controllerDescriptor = mock.Object;
        var controllerBuilder = default( IControllerConventionBuilder );

        mock.Setup( cd => cd.GetCustomAttributes<IApiVersionProvider>() ).Returns( [] );
        controllerDescriptor.Configuration = configuration;
        controllerDescriptor.ControllerType = typeof( UndecoratedController );
        configuration.AddApiVersioning( o => controllerBuilder = o.Conventions.Controller( typeof( UndecoratedController ) ) );
        controllerBuilder.HasDeprecatedApiVersion( 0, 9 )
                         .HasApiVersion( 2, 0 )
                         .AdvertisesApiVersion( 3, 0 )
                         .AdvertisesDeprecatedApiVersion( 3, 0, "Beta" );

        var actionDescriptor = configuration.Services.GetActionSelector().GetActionMapping( controllerDescriptor ).SelectMany( g => g ).Single();

        // act
        controllerBuilder.ApplyTo( controllerDescriptor );

        // assert
        actionDescriptor.GetApiVersionMetadata().Map( Explicit ).Should().BeEquivalentTo(
            new
            {
                IsApiVersionNeutral = false,
                DeclaredApiVersions = Array.Empty<ApiVersion>(),
                SupportedApiVersions = new ApiVersion[] { new( 2, 0 ), new( 3, 0 ) },
                DeprecatedApiVersions = new ApiVersion[] { new( 0, 9 ), new( 3, 0, "Beta" ) },
                ImplementedApiVersions = new ApiVersion[] { new( 0, 9 ), new( 2, 0 ), new( 3, 0 ), new( 3, 0, "Beta" ) },
            } );
    }

    [Fact]
    public void apply_to_should_assign_empty_conventions_to_api_version_neutral_controller()
    {
        // arrange
        var configuration = new HttpConfiguration();
        var mock = new Mock<HttpControllerDescriptor>() { CallBase = true };
        var controllerDescriptor = mock.Object;
        var controllerBuilder = default( IControllerConventionBuilder );

        mock.Setup( cd => cd.GetCustomAttributes<IApiVersionProvider>() ).Returns( [] );
        controllerDescriptor.Configuration = configuration;
        controllerDescriptor.ControllerType = typeof( UndecoratedController );
        configuration.AddApiVersioning( o => controllerBuilder = o.Conventions.Controller( typeof( UndecoratedController ) ) );
        controllerBuilder.HasDeprecatedApiVersion( 0, 9 )
                         .HasApiVersion( 2, 0 )
                         .AdvertisesApiVersion( 3, 0 )
                         .AdvertisesDeprecatedApiVersion( 3, 0, "Beta" )
                         .IsApiVersionNeutral();

        var actionDescriptor = configuration.Services.GetActionSelector().GetActionMapping( controllerDescriptor ).SelectMany( g => g ).Single();

        // act
        controllerBuilder.ApplyTo( controllerDescriptor );

        // assert
        actionDescriptor.GetApiVersionMetadata().Map( Explicit ).Should().BeEquivalentTo(
            new
            {
                IsApiVersionNeutral = true,
                DeclaredApiVersions = Array.Empty<ApiVersion>(),
                SupportedApiVersions = Array.Empty<ApiVersion>(),
                DeprecatedApiVersions = Array.Empty<ApiVersion>(),
                ImplementedApiVersions = Array.Empty<ApiVersion>(),
            } );
    }

    [Fact]
    public void apply_to_should_assign_model_to_controller_from_conventions_and_attributes()
    {
        // arrange
        var configuration = new HttpConfiguration();
        var mock = new Mock<HttpControllerDescriptor>() { CallBase = true };
        var controllerDescriptor = mock.Object;
        var attributes = new Collection<IApiVersionProvider>( typeof( DecoratedController ).GetCustomAttributes().OfType<IApiVersionProvider>().ToList() );
        var controllerBuilder = default( IControllerConventionBuilder );

        mock.Setup( cd => cd.GetCustomAttributes<IApiVersionProvider>() ).Returns( attributes );
        controllerDescriptor.Configuration = configuration;
        controllerDescriptor.ControllerType = typeof( DecoratedController );
        configuration.AddApiVersioning( o => controllerBuilder = o.Conventions.Controller( typeof( DecoratedController ) ) );
        controllerBuilder.HasApiVersion( 1, 0 )
                         .AdvertisesApiVersion( 4, 0 );

        var actionDescriptor = configuration.Services.GetActionSelector().GetActionMapping( controllerDescriptor ).SelectMany( g => g ).Single();

        // act
        controllerBuilder.ApplyTo( controllerDescriptor );

        // assert
        actionDescriptor.GetApiVersionMetadata().Map( Explicit ).Should().BeEquivalentTo(
            new
            {
                IsApiVersionNeutral = false,
                DeclaredApiVersions = Array.Empty<ApiVersion>(),
                SupportedApiVersions = new ApiVersion[] { new( 1, 0 ), new( 2, 0 ), new( 3, 0 ), new( 4, 0 ) },
                DeprecatedApiVersions = new ApiVersion[] { new( 0, 9 ), new( 3, 0, "Beta" ) },
                ImplementedApiVersions = new ApiVersion[] { new( 0, 9 ), new( 1, 0 ), new( 2, 0 ), new( 3, 0 ), new( 3, 0, "Beta" ), new( 4, 0 ) },
            } );
    }
}