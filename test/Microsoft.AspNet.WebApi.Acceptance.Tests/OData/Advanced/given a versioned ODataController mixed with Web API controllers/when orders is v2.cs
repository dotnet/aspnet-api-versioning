namespace given_a_versioned_ODataController_mixed_with_Web_API_controllers
{
    using FluentAssertions;
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Advanced;
    using Microsoft.Web;
    using System.Threading.Tasks;
    using Xunit;

    [Collection( nameof( AdvancedODataCollection ) )]
    public class when_using_OData_for_orders_in_v2 : ODataAcceptanceTest
    {
        [Fact]
        public async Task then_get_should_return_200()
        {
            // arrange


            // act
            var response = await Client.GetAsync( "api/orders?api-version=2.0" );
            var orders = await response.EnsureSuccessStatusCode().Content.ReadAsExampleAsync( new { value = new[] { new { id = 0, customer = "" } } } );

            // assert
            orders.value.Should().BeEquivalentTo( new[] { new { id = 1, customer = "Customer v2.0" } }, options => options.ExcludingMissingMembers() );
        }

        [Fact]
        public async Task then_get_with_key_should_return_200()
        {
            // arrange


            // act
            var response = await Client.GetAsync( "api/orders(42)?api-version=2.0" );
            var order = await response.EnsureSuccessStatusCode().Content.ReadAsExampleAsync( new { id = 0, customer = "" } );

            // assert
            order.Should().BeEquivalentTo( new { id = 42, customer = "Customer v2.0" }, options => options.ExcludingMissingMembers() );
        }

        public when_using_OData_for_orders_in_v2( AdvancedFixture fixture ) : base( fixture ) { }
    }
}