namespace given
{
    using FluentAssertions;
    using Microsoft.Web;
    using Microsoft.Web.OData.Advanced;
    using System;
    using System.Threading.Tasks;
    using Xunit;

    public class _a_v1_orders_ApiController_using_convention_routing : AdvancedAcceptanceTest
    {
        [Fact]
        public async Task _get_should_return_200_without_version()
        {
            // arrange


            // act
            var response = await Client.GetAsync( "api/orders" );
            var orders = await response.EnsureSuccessStatusCode().Content.ReadAsExampleAsync( new[] { new { Id = 0, Customer = "" } } );

            // assert
            orders.ShouldBeEquivalentTo( new[] { new { Id = 1, Customer = "Customer v1.0" } } );
        }

        [Fact]
        public async Task _get_should_return_200()
        {
            // arrange


            // act
            var response = await Client.GetAsync( "api/orders?api-version=1.0" );
            var orders = await response.EnsureSuccessStatusCode().Content.ReadAsExampleAsync( new[] { new { Id = 0, Customer = "" } } );

            // assert
            orders.ShouldBeEquivalentTo( new[] { new { Id = 1, Customer = "Customer v1.0" } } );
        }

        [Fact]
        public async Task _get_with_key_should_return_200_without_version()
        {
            // arrange


            // act
            var response = await Client.GetAsync( "api/orders/42" );
            var order = await response.EnsureSuccessStatusCode().Content.ReadAsExampleAsync( new { Id = 0, Customer = "" } );

            // assert
            order.ShouldBeEquivalentTo( new { Id = 42, Customer = "Customer v1.0" } );
        }

        [Fact]
        public async Task _get_with_key_should_return_200()
        {
            // arrange


            // act
            var response = await Client.GetAsync( "api/orders/42?api-version=1.0" );
            var order = await response.EnsureSuccessStatusCode().Content.ReadAsExampleAsync( new { Id = 0, Customer = "" } );

            // assert
            order.ShouldBeEquivalentTo( new { Id = 42, Customer = "Customer v1.0" } );
        }
    }
}