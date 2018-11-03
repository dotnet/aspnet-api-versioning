namespace Microsoft.AspNet.OData
{
    using FluentAssertions;
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.Web.Http;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.Dispatcher;
    using Xunit;

    public class VersionedMetadataControllerTest
    {
        [Fact]
        public async Task options_should_return_expected_headers()
        {
            // arrange
            var configuration = new HttpConfiguration();
            var builder = new VersionedODataModelBuilder( configuration );
            var metadata = new VersionedMetadataController() { Configuration = configuration };
            var controllerTypeResolver = new Mock<IHttpControllerTypeResolver>();
            var controllerTypes = new List<Type>() { typeof( Controller1 ), typeof( Controller2 ), typeof( VersionedMetadataController ) };

            controllerTypeResolver.Setup( ctr => ctr.GetControllerTypes( It.IsAny<IAssembliesResolver>() ) ).Returns( controllerTypes );
            configuration.Services.Replace( typeof( IHttpControllerTypeResolver ), controllerTypeResolver.Object );
            configuration.AddApiVersioning();

            var models = builder.GetEdmModels();
            var request = new HttpRequestMessage( new HttpMethod( "OPTIONS" ), "http://localhost/$metadata" );
            var response = default( HttpResponseMessage );

            configuration.MapVersionedODataRoutes( "odata", null, models );

            using ( var server = new HttpServer( configuration ) )
            using ( var client = new HttpClient( server ) )
            {
                // act
                response = ( await client.SendAsync( request ) ).EnsureSuccessStatusCode();
            }

            // assert
            response.Headers.GetValues( "OData-Version" ).Single().Should().Be( "4.0" );
            response.Headers.GetValues( "api-supported-versions" ).Single().Should().Be( "1.0, 2.0, 3.0" );
            response.Headers.GetValues( "api-deprecated-versions" ).Single().Should().Be( "3.0-Beta" );
            response.Content.Headers.Allow.Should().BeEquivalentTo( "GET", "OPTIONS" );
        }

        [ApiVersion( "1.0" )]
        [ApiVersion( "2.0" )]
        sealed class Controller1 : ODataController
        {
            public IHttpActionResult Get() => Ok();
        }

        [ApiVersion( "2.0", Deprecated = true )]
        [ApiVersion( "3.0-Beta", Deprecated = true )]
        [ApiVersion( "3.0" )]
        sealed class Controller2 : ODataController
        {
            public IHttpActionResult Get() => Ok();
        }
    }
}