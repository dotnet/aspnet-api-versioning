// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace given_a_versioned_ApiController_mixed_with_OData_controllers;

using Asp.Versioning;
using Asp.Versioning.OData.Advanced;

public class when_orders_is_v3 : AdvancedAcceptanceTest
{
    [Fact]
    public async Task then_get_should_return_200()
    {
        // arrange
        var example = new[] { new { Id = 0, Customer = "" } };


        // act
        var response = await GetAsync( "api/orders?api-version=3.0" );
        var orders = await response.EnsureSuccessStatusCode().Content.ReadAsExampleAsync( example );

        // assert
        orders.Should().BeEquivalentTo( new[] { new { Id = 1, Customer = "Customer v3.0" } } );
    }

    [Fact]
    public async Task then_get_with_key_should_return_200_for_an_unspecified_version()
    {
        // arrange
        var example = new { Id = 0, Customer = "" };


        // act
        var response = await GetAsync( "api/orders/42?api-version=3.0" );
        var order = await response.EnsureSuccessStatusCode().Content.ReadAsExampleAsync( example );

        // assert
        order.Should().BeEquivalentTo( new { Id = 42, Customer = "Customer v3.0" } );
    }

    public when_orders_is_v3( AdvancedFixture fixture ) : base( fixture ) { }
}