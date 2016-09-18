namespace given
{
    using FluentAssertions;
    using Microsoft.Web;
    using Microsoft.Web.OData.Advanced;
    using System;
    using System.Threading.Tasks;
    using Xunit;

    public class _a_v2_orders_ODataController : AdvancedAcceptanceTest
    {
        [Fact]
        public async Task _get_should_return_200()
        {
            // arrange


            // act
            var response = await Client.GetAsync( "api/orders?api-version=2.0" );
            var orders = await response.EnsureSuccessStatusCode().Content.ReadAsExampleAsync( new { value = new[] { new { id = 0, customer = "" } } } );

            // assert
            orders.value.ShouldBeEquivalentTo( new[] { new { id = 1, customer = "Customer v2.0" } }, options => options.ExcludingMissingMembers() );
        }

        [Fact]
        public async Task _get_with_key_should_return_200()
        {
            // arrange


            // act
            var response = await Client.GetAsync( "api/orders(42)?api-version=2.0" );
            var order = await response.EnsureSuccessStatusCode().Content.ReadAsExampleAsync( new { id = 0, customer = "" } );

            // assert
            order.ShouldBeEquivalentTo( new { id = 42, customer = "Customer v2.0" }, options => options.ExcludingMissingMembers() );
        }
    }
}
