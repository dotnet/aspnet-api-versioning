// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

using System.Web.Http;
using System.Web.Http.Controllers;
using static Asp.Versioning.ApiVersionMapping;

public partial class ApiVersionConventionBuilderTest
{
    [Fact]
    public void apply_should_apply_configured_conventions()
    {
        // arrange
        var configuration = new HttpConfiguration();
        var controllerDescriptor = new HttpControllerDescriptor( configuration, "Undecorated", typeof( v2.UndecoratedController ) );
        var conventionBuilder = new ApiVersionConventionBuilder();

        conventionBuilder.Add( new VersionByNamespaceConvention() );
        configuration.AddApiVersioning( o => o.Conventions = conventionBuilder );

        var actionDescriptor = configuration.Services.GetActionSelector().GetActionMapping( controllerDescriptor ).SelectMany( g => g ).Single();

        // act
        conventionBuilder.ApplyTo( controllerDescriptor );

        // assert
        actionDescriptor.GetApiVersionMetadata().MappingTo( new ApiVersion( 2, 0 ) ).Should().Be( Implicit );
    }
}