namespace Microsoft.AspNetCore.Mvc.ApiExplorer
{
    using FluentAssertions;
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.AspNet.OData.Simulators.Models;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.OData.Edm;
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

        static ApiDescription CreateApiDescription( IEdmModel model )
        {
            var apiDescription = new ApiDescription()
            {
                ActionDescriptor = new ControllerActionDescriptor() { ControllerName = "Orders" },
                Properties = { [typeof( IEdmModel )] = model }
            };

            return apiDescription;
        }
    }
}