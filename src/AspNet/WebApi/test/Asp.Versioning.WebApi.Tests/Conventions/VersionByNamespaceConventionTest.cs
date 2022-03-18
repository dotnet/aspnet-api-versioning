// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

using System.Collections.ObjectModel;
using System.Reflection;
using System.Web.Http.Controllers;
using static Moq.Times;

public partial class VersionByNamespaceConventionTest
{
    [Theory]
    [MemberData( nameof( NamespaceAsVersionData ) )]
    public void apply_should_infer_supported_api_version_from_namespace( string @namespace, string versionText )
    {
        // arrange
        var apiVersion = ApiVersionParser.Default.Parse( versionText );
        var controllerType = new TestType( @namespace );
        var attributes = Array.Empty<Attribute>();
        var controllerModel = new TestHttpControllerDescriptor( controllerType.GetTypeInfo(), attributes );
        var controller = new Mock<IControllerConventionBuilder>();
        var convention = new VersionByNamespaceConvention();

        controller.Setup( c => c.HasApiVersion( It.IsAny<ApiVersion>() ) );

        // act
        var applied = convention.Apply( controller.Object, controllerModel );

        // assert
        applied.Should().BeTrue();
        controller.Verify( c => c.HasApiVersion( apiVersion ), Once() );
    }

    [Theory]
    [MemberData( nameof( NamespaceAsVersionData ) )]
    public void apply_should_infer_deprecated_api_version_from_namespace( string @namespace, string versionText )
    {
        // arrange
        var apiVersion = ApiVersionParser.Default.Parse( versionText );
        var controllerType = new TestType( @namespace );
        var attributes = new Attribute[] { new ObsoleteAttribute( "Deprecated" ) };
        var controllerModel = new TestHttpControllerDescriptor( controllerType.GetTypeInfo(), attributes );
        var controller = new Mock<IControllerConventionBuilder>();
        var convention = new VersionByNamespaceConvention();

        controller.Setup( c => c.HasDeprecatedApiVersion( It.IsAny<ApiVersion>() ) );

        // act
        var applied = convention.Apply( controller.Object, controllerModel );

        // assert
        applied.Should().BeTrue();
        controller.Verify( c => c.HasDeprecatedApiVersion( apiVersion ), Once() );
    }

    [Theory]
    [InlineData( "Contoso.Api.v1.Controllers.v1" )]
    [InlineData( "Contoso.Api.v1_1.Controllers.v1" )]
    [InlineData( "Contoso.Api.v2_0.Controllers.v2" )]
    [InlineData( "Contoso.Api.v20180401.Controllers.v1" )]
    [InlineData( "Contoso.Api.v2018_04_01.Controllers.v2_0_Beta" )]
    [InlineData( "v2018_04_01.Controllers.v2_0_RC" )]
    public void apply_should_throw_exception_for_ambiguous_api_versions_in_namespace( string @namespace )
    {
        // arrange
        var controllerType = new TestType( @namespace );
        var attributes = Array.Empty<Attribute>();
        var controllerModel = new TestHttpControllerDescriptor( controllerType.GetTypeInfo(), attributes );
        var convention = new VersionByNamespaceConvention();

        // act
        Action apply = () => convention.Apply( Mock.Of<IControllerConventionBuilder>(), controllerModel );

        // assert
        apply.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void apply_should_ignore_unmatched_namespace()
    {
        // arrange
        var controllerType = new TestType( "Contoso.Api.Controllers" );
        var attributes = Array.Empty<Attribute>();
        var controllerModel = new TestHttpControllerDescriptor( controllerType.GetTypeInfo(), attributes );
        var convention = new VersionByNamespaceConvention();

        // act
        var applied = convention.Apply( Mock.Of<IControllerConventionBuilder>(), controllerModel );

        // assert
        applied.Should().BeFalse();
    }

    private sealed class TestHttpControllerDescriptor : HttpControllerDescriptor
    {
        private readonly IReadOnlyList<Attribute> attributes;

        internal TestHttpControllerDescriptor( Type controllerType, IReadOnlyList<Attribute> attributes )
        {
            ControllerType = controllerType;
            this.attributes = attributes;
        }

        public override Collection<T> GetCustomAttributes<T>( bool inherit ) => new( attributes.OfType<T>().ToArray() );
    }
}