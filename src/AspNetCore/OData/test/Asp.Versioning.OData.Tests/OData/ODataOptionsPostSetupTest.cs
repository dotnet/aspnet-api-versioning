// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData;

using Asp.Versioning.Routing;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.AspNetCore.OData.Routing.Parser;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using ODataSetup = Microsoft.AspNetCore.OData.ODataOptionsSetup;

public class ODataOptionsPostSetupTest
{
    [Fact]
    public void post_configure_should_replace_metadata_routing_convention()
    {
        // arrange
        var options = new ODataOptions();
        using var loggerFactory = new NullLoggerFactory();
        var parser = Mock.Of<IODataPathTemplateParser>();
        var setup = new ODataSetup( loggerFactory, parser );
        var postSetup = new ODataOptionsPostSetup( loggerFactory, parser );

        setup.Configure( options );

        // act
        postSetup.PostConfigure( Options.DefaultName, options );

        // assert
        options.Conventions
               .Any( c => c.GetType().Equals( typeof( MetadataRoutingConvention ) ) )
               .Should()
               .BeFalse();

        options.Conventions
               .OfType<VersionedMetadataRoutingConvention>()
               .Any()
               .Should()
               .BeTrue();
    }

    [Fact]
    public void post_configure_should_replace_attribute_routing_convention()
    {
        // arrange
        var options = new ODataOptions();
        using var loggerFactory = new NullLoggerFactory();
        var parser = Mock.Of<IODataPathTemplateParser>();
        var setup = new ODataSetup( loggerFactory, parser );
        var postSetup = new ODataOptionsPostSetup( loggerFactory, parser );

        setup.Configure( options );

        // act
        postSetup.PostConfigure( Options.DefaultName, options );

        // assert
        options.Conventions
               .Any( c => c.GetType().Equals( typeof( AttributeRoutingConvention ) ) )
               .Should()
               .BeFalse();

        options.Conventions
               .OfType<VersionedAttributeRoutingConvention>()
               .Any()
               .Should()
               .BeTrue();
    }
}