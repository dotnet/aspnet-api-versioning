namespace given_a_versioned_ControllerBase_mixed_with_OData_controllers
{
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.OData;
    using Microsoft.AspNetCore.OData.Advanced;
    using System.Threading.Tasks;
    using Xunit;

    [Collection( nameof( AdvancedODataCollection ) )]
    public class when_orders_is_v1 : ODataAcceptanceTest
    {
        [Fact]
        public async Task then_get_should_return_200_for_an_unspecified_version()
        {
            // arrange


            // act
            var response = await Client.GetAsync( "api/orders" );
            var orders = await response.EnsureSuccessStatusCode().Content.ReadAsExampleAsync( new[] { new { Id = 0, Customer = "" } } );

            // assert
            orders.Should().BeEquivalentTo( new[] { new { Id = 1, Customer = "Customer v1.0" } } );
        }

        [Fact]
        public async Task then_get_should_return_200()
        {
            // arrange


            // act
            var response = await Client.GetAsync( "api/orders?api-version=1.0" );
            var orders = await response.EnsureSuccessStatusCode().Content.ReadAsExampleAsync( new[] { new { Id = 0, Customer = "" } } );

            // assert
            orders.Should().BeEquivalentTo( new[] { new { Id = 1, Customer = "Customer v1.0" } } );
        }

        [Fact]
        public async Task then_get_with_key_should_return_200_for_an_unspecified_version()
        {
            // arrange


            // act
            var response = await Client.GetAsync( "api/orders/42" );
            var order = await response.EnsureSuccessStatusCode().Content.ReadAsExampleAsync( new { Id = 0, Customer = "" } );

            // assert
            order.Should().BeEquivalentTo( new { Id = 42, Customer = "Customer v1.0" } );
        }

        [Fact]
        public async Task then_get_with_key_should_return_200()
        {
            // arrange


            // act
            var response = await Client.GetAsync( "api/orders/42?api-version=1.0" );
            var order = await response.EnsureSuccessStatusCode().Content.ReadAsExampleAsync( new { Id = 0, Customer = "" } );

            // assert
            order.Should().BeEquivalentTo( new { Id = 42, Customer = "Customer v1.0" } );
        }

        public when_orders_is_v1( AdvancedFixture fixture ) : base( fixture ) { }
    }
}