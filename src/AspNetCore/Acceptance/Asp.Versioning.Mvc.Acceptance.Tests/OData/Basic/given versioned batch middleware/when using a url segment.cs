// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0079
#pragma warning disable CA2000 // Dispose objects before losing scope

namespace given_versioned_batch_middleware;

using Asp.Versioning.OData.Basic;
using static System.Net.HttpStatusCode;

public class when_using_a_url_segment : BatchAcceptanceTest
{
    [Fact]
    public async Task then_2_different_versions_should_return_200()
    {
        // arrange
        using var request = NewBatch(
            "api/$batch",
            NewGet( "v1/people/42" ),
            NewGet( "v2/people/42" ) );

        // act
        var response = await Client.SendAsync( request );

        // assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var multipart = await response.Content.ReadAsMultipartAsync();
        var contents = multipart.Contents;

        contents.Should().HaveCount( 2 );

        const string NoEmailInV1 = default;
        var example = new { id = 0, firstName = "", lastName = "", email = "" };
        var person1 = await ReadAsAsync( multipart.Contents[0], example );
        var person2 = await ReadAsAsync( multipart.Contents[1], example );

        person1.Should().BeEquivalentTo(
            new
            {
                id = 42,
                firstName = "Bill",
                lastName = "Mei",
                email = NoEmailInV1,
            } );
        person2.Should().BeEquivalentTo(
            new
            {
                id = 42,
                firstName = "Bill",
                lastName = "Mei",
                email = "bill.mei@somewhere.com",
            } );
    }

    [Fact]
    public async Task then_2_different_entity_sets_should_return_200()
    {
        // arrange
        var expected = new
        {
            id = 42,
            firstName = "Bill",
            lastName = "Mei",
            email = "bill.mei@somewhere.com",
            phone = "555-555-5555",
        };
        using var request = NewBatch(
            "api/$batch",
            NewGet( "v3/people/42" ),
            NewGet( "v1/orders/42" ) );

        // act
        var response = await Client.SendAsync( request );

        // assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var multipart = await response.Content.ReadAsMultipartAsync();
        var contents = multipart.Contents;

        contents.Should().HaveCount( 2 );

        var person = await ReadAsAsync( multipart.Contents[0], expected );
        var order = await ReadAsAsync( multipart.Contents[1], new { id = 0, customer = "" } );

        person.Should().BeEquivalentTo( expected );
        order.Should().BeEquivalentTo( new { id = 42, customer = "Bill Mei" } );
    }

    [Fact]
    public async Task then_explicit_versions_should_succeed()
    {
        // arrange
        var customer = new
        {
            id = 42,
            firstName = "Bill",
            lastName = "Mei",
            email = "bill.mei@somewhere.com",
            phone = "555-555-5555",
        };
        using var request = NewBatch(
            "v1/$batch",
            NewPut( "v3/customers/42", customer ),
            NewDelete( "v3/customers/42" ) );

        // act
        var response = await Client.SendAsync( request );

        // assert
        response.IsSuccessStatusCode.Should().BeTrue();

        await foreach ( var subresponse in ReadAsResponses( response ) )
        {
            subresponse.StatusCode.Should().Be( NoContent );
        }
    }

    public when_using_a_url_segment( BasicFixture fixture ) : base( fixture ) { }
}