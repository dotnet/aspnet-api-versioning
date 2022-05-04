// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.OData.ModelBuilder.Config;
using System.Reflection;

public partial class ODataQueryOptionsConventionBuilderTest
{
    [Fact]
    public void apply_should_apply_configured_conventions()
    {
        // arrange
        var description = new ApiDescription()
        {
            ActionDescriptor = new ControllerActionDescriptor()
            {
                ControllerTypeInfo = typeof( StubController ).GetTypeInfo(),
                MethodInfo = typeof( StubController ).GetTypeInfo()
                                                     .GetRuntimeMethod(
                                                         nameof( StubController.Get ),
                                                         Type.EmptyTypes ),
            },
            HttpMethod = "GET",
        };
        var builder = new ODataQueryOptionsConventionBuilder();
        var settings = new ODataQueryOptionSettings()
        {
            DescriptionProvider = builder.DescriptionProvider,
            DefaultQuerySettings = new DefaultQuerySettings(),
            ModelMetadataProvider = Mock.Of<IModelMetadataProvider>(),
        };
        var convention = new Mock<IODataQueryOptionsConvention>();

        convention.Setup( c => c.ApplyTo( It.IsAny<ApiDescription>() ) );
        builder.Add( convention.Object );

        // act
        builder.ApplyTo( new[] { description }, settings );

        // assert
        convention.Verify( c => c.ApplyTo( description ), Times.Once() );
    }
}