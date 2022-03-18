// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace given_a_versioned_ODataController_mixed_with_Web_API_controllers;

using Asp.Versioning;
using Asp.Versioning.OData.Advanced;

public class when_orders_is_v2 : AdvancedAcceptanceTest
{
    [Fact]
    public async Task then_get_should_return_200()
    {
        // arrange
        var example = new { value = new[] { new { id = 0, customer = "" } } };


        // act
        var response = await GetAsync( "api/orders?api-version=2.0" );
        var orders = await response.EnsureSuccessStatusCode().Content.ReadAsExampleAsync( example );

        // assert
        orders.value.Should().BeEquivalentTo(
            new[] { new { id = 1, customer = "Customer v2.0" } },
            options => options.ExcludingMissingMembers() );
    }

    [Fact]
    public async Task then_get_with_key_should_return_200()
    {
        // arrange
        var example = new { id = 0, customer = "" };


        // act
        var response = await GetAsync( "api/orders/42?api-version=2.0" );
        var order = await response.EnsureSuccessStatusCode().Content.ReadAsExampleAsync( example );

        // assert
        order.Should().BeEquivalentTo(
            new { id = 42, customer = "Customer v2.0" },
            options => options.ExcludingMissingMembers() );
    }

    public when_orders_is_v2( AdvancedFixture fixture ) : base( fixture ) { }
}