namespace Microsoft.AspNetCore.Mvc.Versioning.Conventions
{
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc.ApplicationModels;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Xunit;
    using static Moq.Times;

    public class VersionByNamespaceConventionTest
    {
        [Theory]
        [MemberData( nameof( NamespaceAsVersionData ) )]
        public void apply_should_infer_supported_api_version_from_namespace( string @namespace, string versionText )
        {
            // arrange
            var apiVersion = ApiVersion.Parse( versionText );
            var controllerType = new TestType( @namespace );
            var attributes = Array.Empty<object>();
            var controllerModel = new ControllerModel( controllerType.GetTypeInfo(), attributes );
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
        [MemberData(nameof( NamespaceAsVersionData ) )]
        public void apply_should_infer_deprecated_api_version_from_namespace( string @namespace, string versionText )
        {
            // arrange
            var apiVersion = ApiVersion.Parse( versionText );
            var controllerType = new TestType( @namespace );
            var attributes = new object[] { new ObsoleteAttribute( "Deprecated" ) };
            var controllerModel = new ControllerModel( controllerType.GetTypeInfo(), attributes );
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
            var attributes = Array.Empty<object>();
            var controllerModel = new ControllerModel( controllerType.GetTypeInfo(), attributes );
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
            var attributes = Array.Empty<object>();
            var controllerModel = new ControllerModel( controllerType.GetTypeInfo(), attributes );
            var controller = new Mock<IControllerConventionBuilder>();
            var convention = new VersionByNamespaceConvention();

            // act
            var applied = convention.Apply( Mock.Of<IControllerConventionBuilder>(), controllerModel );

            // assert
            applied.Should().BeFalse();
        }

        public static IEnumerable<object[]> NamespaceAsVersionData
        {
            get
            {
                yield return new object[] { "v1", "1.0" };
                yield return new object[]{ "v1RC", "1.0-RC" };
                yield return new object[]{ "v20180401", "2018-04-01" };
                yield return new object[]{ "v20180401_Beta", "2018-04-01-Beta" };
                yield return new object[]{ "v20180401Beta", "2018-04-01-Beta" };
                yield return new object[]{ "Contoso.Api.v1.Controllers", "1.0" };
                yield return new object[]{ "Contoso.Api.v1_1.Controllers", "1.1" };
                yield return new object[]{ "Contoso.Api.v0_9_Beta.Controllers", "0.9-Beta" };
                yield return new object[]{ "Contoso.Api.v20180401.Controllers", "2018-04-01" };
                yield return new object[]{ "Contoso.Api.v2018_04_01.Controllers", "2018-04-01" };
                yield return new object[]{ "Contoso.Api.v20180401_Beta.Controllers", "2018-04-01-Beta" };
                yield return new object[]{ "Contoso.Api.v2018_04_01_Beta.Controllers", "2018-04-01-Beta" };
                yield return new object[]{ "Contoso.Api.v2018_04_01_1_0_Beta.Controllers", "2018-04-01.1.0-Beta" };
                yield return new object[]{ "MyRestaurant.Vegetarian.Food.v1_1.Controllers", "1.1" };
                yield return new object[]{ "VersioningSample.V5.Controllers", "5.0" };
            }
        }

        sealed class TestType : TypeDelegator
        {
            internal TestType( string @namespace ) => Namespace = @namespace;

            public override string Namespace { get; }
        }
    }
}