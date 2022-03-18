// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

using Asp.Versioning.Description;
using Microsoft.OData.Edm;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Description;

public partial class ODataQueryOptionsConventionBuilderTest
{
    [Fact]
    public void apply_should_apply_configured_conventions()
    {
        // arrange
        var controller = new HttpControllerDescriptor( new HttpConfiguration(), "Stub", typeof( StubController ) );
        var action = new ReflectedHttpActionDescriptor( controller, typeof( StubController ).GetMethod( nameof( StubController.Get ) ) );
        var description = new VersionedApiDescription()
        {
            ActionDescriptor = action,
            HttpMethod = HttpMethod.Get,
            ResponseDescription = new() { ResponseType = typeof( object ) },
            Properties = { [typeof( IEdmModel )] = new EdmModel() },
        };
        var builder = new ODataQueryOptionsConventionBuilder();
        var settings = new ODataQueryOptionSettings() { DescriptionProvider = builder.DescriptionProvider };
        var convention = new Mock<IODataQueryOptionsConvention>();

        convention.Setup( c => c.ApplyTo( It.IsAny<ApiDescription>() ) );
        builder.Add( convention.Object );

        // act
        builder.ApplyTo( new[] { description }, settings );

        // assert
        convention.Verify( c => c.ApplyTo( description ), Times.Once() );
    }
}