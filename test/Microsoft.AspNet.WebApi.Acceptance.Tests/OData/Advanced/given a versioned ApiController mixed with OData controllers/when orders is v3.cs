namespace given_a_versioned_ApiController_mixed_with_OData_controllers
{
    using FluentAssertions;
    using Microsoft.Web;
    using Microsoft.Web.OData.Advanced;
    using System;
    using System.Threading.Tasks;
    using Xunit;

    public class when_orders_is_v3 : AdvancedAcceptanceTest
    {
        [Fact]
        public async Task then_get_should_return_200()
        {
            // arrange


            // act
            var response = await Client.GetAsync( "api/orders?api-version=3.0" );
            var orders = await response.EnsureSuccessStatusCode().Content.ReadAsExampleAsync( new[] { new { Id = 0, Customer = "" } } );

            // assert
            orders.ShouldBeEquivalentTo( new[] { new { Id = 1, Customer = "Customer v3.0" } } );
        }

        [Fact]
        public async Task then_get_with_key_should_return_200_for_an_unspecified_version()
        {
            // arrange


            // act
            var response = await Client.GetAsync( "api/orders/42?api-version=3.0" );
            var order = await response.EnsureSuccessStatusCode().Content.ReadAsExampleAsync( new { Id = 0, Customer = "" } );

            // assert
            order.ShouldBeEquivalentTo( new { Id = 42, Customer = "Customer v3.0" } );
        }
    }
}