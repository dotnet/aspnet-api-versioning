namespace Microsoft.Web.OData.Controllers
{
    using FluentAssertions;
    using Http;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.Dispatcher;
    using System.Web.OData;
    using Xunit;

    public class VersionedMetadataControllerTest
    {
        [ApiVersion( "1.0" )]
        [ApiVersion( "2.0" )]
        private sealed class Controller1 : ODataController
        {
        }

        [ApiVersion( "2.0", Deprecated = true )]
        [ApiVersion( "3.0-Beta", Deprecated = true )]
        [ApiVersion( "3.0" )]
        private sealed class Controller2 : ODataController
        {
        }

        [Fact]
        public async Task options_should_return_expected_headers()
        {
            // arrange
            var configuration = new HttpConfiguration();
            var controllerTypeResolver = new Mock<IHttpControllerTypeResolver>();
            var controllerTypes = new List<Type>() { typeof( Controller1 ), typeof( Controller2 ) };

            controllerTypeResolver.Setup( ctr => ctr.GetControllerTypes( It.IsAny<IAssembliesResolver>() ) ).Returns( controllerTypes );
            configuration.Services.Replace( typeof( IHttpControllerTypeResolver ), controllerTypeResolver.Object );

            var metadata = new VersionedMetadataController() { Configuration = configuration };

            // act
            var response = await metadata.GetOptions().ExecuteAsync( CancellationToken.None );

            // assert
            response.Headers.GetValues( "OData-Version" ).Single().Should().Be( "4.0" );
            response.Headers.GetValues( "api-supported-versions" ).Single().Should().Be( "1.0, 2.0, 3.0" );
            response.Headers.GetValues( "api-deprecated-versions" ).Single().Should().Be( "3.0-Beta" );
            response.Content.Headers.Allow.Should().BeEquivalentTo( "GET", "OPTIONS" );
        }
    }
}
