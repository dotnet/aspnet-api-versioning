namespace Microsoft.Web.Http.Controllers
{
    using FluentAssertions;
    using Moq;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Net.Http;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using Versioning;
    using Xunit;
    using static System.Net.Http.HttpMethod;

    public class ApiVersionActionSelectorTest
    {
        sealed class TestApiVersionActionSelector : ApiVersionActionSelector
        {
            internal HttpActionDescriptor InvokeSelectActionVersion( HttpControllerContext controllerContext, IReadOnlyList<HttpActionDescriptor> candidateActions ) =>
                SelectActionVersion( controllerContext, candidateActions );
        }

        static HttpActionDescriptor CreateActionDescriptor( string version )
        {
            var configuration = new HttpConfiguration();
            var controllerType = typeof( IHttpController );
            var controllerDescriptor = new Mock<HttpControllerDescriptor>( configuration, "Test", controllerType ) { CallBase = true };
            var actionDescriptor = new Mock<HttpActionDescriptor>() { CallBase = true };
            var attribute = new ApiVersionAttribute( version );

            controllerDescriptor.Setup( cd => cd.GetCustomAttributes<IApiVersionNeutral>( It.IsAny<bool>() ) )
                                .Returns( () => new Collection<IApiVersionNeutral>() );

            actionDescriptor.Setup( ad => ad.GetCustomAttributes<IApiVersionProvider>( It.IsAny<bool>() ) )
                            .Returns( () => new Collection<IApiVersionProvider>() { attribute } );

            var newActionDescriptor = actionDescriptor.Object;

            newActionDescriptor.ControllerDescriptor = controllerDescriptor.Object;
            newActionDescriptor.Properties[typeof( ApiVersionModel )] = new ApiVersionModel( attribute.Versions[0] );

            return newActionDescriptor;
        }

        public static IEnumerable<object[]> SelectActionVersionData
        {
            get
            {
                var candidates = new List<HttpActionDescriptor>()
                {
                    CreateActionDescriptor( "1.0" ),
                    CreateActionDescriptor( "2.0" ),
                    CreateActionDescriptor( "3.0" )
                };

                yield return new object[] { candidates, "1.0", candidates[0] };
                yield return new object[] { candidates, "2.0", candidates[1] };
                yield return new object[] { candidates, "3.0", candidates[2] };
            }
        }

        [Theory]
        [MemberData( nameof( SelectActionVersionData ) )]
        public void select_action_version_should_return_expected_result( IReadOnlyList<HttpActionDescriptor> candidates, string version, HttpActionDescriptor expectedAction )
        {
            // arrange
            var configuration = new HttpConfiguration();
            var request = new HttpRequestMessage( Get, "http://localhost/api/test?api-version=" + version );
            var context = new HttpControllerContext() { Request = request };
            var selector = new TestApiVersionActionSelector();

            configuration.AddApiVersioning();
            request.SetConfiguration( configuration );

            // act
            var selectedAction = selector.InvokeSelectActionVersion( context, candidates );

            // assert
            selectedAction.Should().Be( expectedAction );
        }
    }
}