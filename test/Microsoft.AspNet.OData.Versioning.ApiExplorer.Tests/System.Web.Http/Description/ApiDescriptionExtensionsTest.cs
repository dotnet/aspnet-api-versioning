namespace System.Web.Http.Description
{
    using FluentAssertions;
    using Microsoft.OData.Edm;
    using Microsoft.Web.Http;
    using Microsoft.Web.Http.Description;
    using Microsoft.Web.Http.Simulators.Models;
    using System.Reflection;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.OData.Builder;
    using Xunit;

    public class ApiDescriptionExtensionsTest
    {
        [Fact]
        public void edm_model_should_be_retrieved_from_properties()
        {
            // arrange
            var model = CreateEdmModel();
            var apiDescription = CreateApiDescription( model );

            // act
            var result = apiDescription.EdmModel();

            // assert
            result.Should().BeSameAs( model );
        }

        [Fact]
        public void entity_set_should_be_retrieved_from_properties()
        {
            // arrange
            var model = CreateEdmModel();
            var entitySet = model.EntityContainer.FindEntitySet( "Orders" );
            var apiDescription = CreateApiDescription( model );

            // act
            var result = apiDescription.EntitySet();

            // assert
            result.Should().BeSameAs( entitySet );
        }

        [Fact]
        public void entity_type_should_be_retrieved_from_properties()
        {
            // arrange
            var model = CreateEdmModel();
            var entityType = model.EntityContainer.FindEntitySet( "Orders" ).EntityType();
            var apiDescription = CreateApiDescription( model );

            // act
            var result = apiDescription.EntityType();

            // assert
            result.Should().BeSameAs( entityType );
        }

        static IEdmModel CreateEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<Order>( "Orders" );
            return builder.GetEdmModel();
        }

        static VersionedApiDescription CreateApiDescription( IEdmModel model )
        {
            var configuration = new HttpConfiguration();
            var controllerType = typeof( Microsoft.Web.Http.Simulators.V1.OrdersController );
            var actionMethod = controllerType.GetRuntimeMethod( "Get", new[] { typeof( int ) } );
            var controllerDescriptor = new HttpControllerDescriptor( configuration, "Orders", controllerType );
            var actionDescriptor = new ReflectedHttpActionDescriptor( controllerDescriptor, actionMethod );
            var apiDescription = new VersionedApiDescription()
            {
                ActionDescriptor = actionDescriptor,
                ApiVersion = new ApiVersion( 1, 0 ),
                Properties = { [typeof( IEdmModel )] = model }
            };

            return apiDescription;
        }
    }
}