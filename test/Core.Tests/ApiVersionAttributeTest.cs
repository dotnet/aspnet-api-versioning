namespace Microsoft.AspNetCore.Mvc
{
    using ApplicationModels;
    using FluentAssertions;
    using System.Linq;
    using System.Reflection;
    using Versioning;
    using Xunit;
    using static System.Type;

    public class ApiVersionAttributeTest
    {
        [Fact]
        public void convention_should_apply_api_version_model()
        {
            // arrange
            var supported = new[] { new ApiVersion( 1, 0 ), new ApiVersion( 2, 0 ), new ApiVersion( 3, 0 ) };
            var deprecated = new[] { new ApiVersion( 0, 9 ) };
            var model = new ApiVersionModel( supported, deprecated );
            var type = typeof( object );
            var attributes = new object[]
            {
                new ApiVersionAttribute( "1.0" ),
                new ApiVersionAttribute( "2.0" ),
                new ApiVersionAttribute( "3.0" ),
                new ApiVersionAttribute( "0.9" ) { Deprecated = true }
            };
            var actionMethod = type.GetRuntimeMethod( nameof( object.ToString ), EmptyTypes );
            var controller = new ControllerModel( type.GetTypeInfo(), attributes )
            {
                Actions =
                {
                    new ActionModel( actionMethod, attributes )
                }
            };
            var convention = (ApiVersionAttribute) attributes[0];

            // act
            convention.Apply( controller );

            // assert
            controller.GetProperty<ApiVersionModel>().ShouldBeEquivalentTo(
                new
                {
                    IsApiVersionNeutral = false,
                    DeclaredApiVersions = deprecated.Union( supported ).ToArray(),
                    ImplementedApiVersions = deprecated.Union( supported ).ToArray(),
                    SupportedApiVersions = supported,
                    DeprecatedApiVersions = deprecated
                } );
            controller.Actions.Single().GetProperty<ApiVersionModel>().ShouldBeEquivalentTo(
                new
                {
                    IsApiVersionNeutral = false,
                    DeclaredApiVersions = deprecated.Union( supported ).ToArray(),
                    ImplementedApiVersions = deprecated.Union( supported ).ToArray(),
                    SupportedApiVersions = supported,
                    DeprecatedApiVersions = deprecated
                } );
        }
    }
}
