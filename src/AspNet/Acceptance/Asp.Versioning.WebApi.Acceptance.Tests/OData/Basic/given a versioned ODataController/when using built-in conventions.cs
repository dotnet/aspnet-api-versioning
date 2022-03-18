// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE1006 // Naming Styles (X2D allowed for '-')

namespace given_a_versioned_ODataController;

using Asp.Versioning.OData.Basic;
using static System.Net.HttpStatusCode;

public class when_using_builtX2Din_conventions : BasicAcceptanceTest
{
    [Theory]
    [InlineData( "api/weatherforecasts?api-version=1.0" )]
    [InlineData( "api/weatherforecasts/42?api-version=1.0" )]
    public async Task then_get_should_return_200( string requestUrl )
    {
        // arrange

        // act
        var response = await GetAsync( requestUrl );

        // assert
        response.StatusCode.Should().Be( OK );
    }

    [Fact]
    public async Task then_post_should_return_200()
    {
        // arrange
        var forecast = new { date = DateTime.Today, temperature = 42d, summary = "Test" };

        // act
        var response = await PostAsync( "api/weatherforecasts?api-version=1.0", forecast );

        // assert
        response.StatusCode.Should().Be( OK );
    }

    [Fact]
    public async Task then_put_should_return_200()
    {
        // arrange
        var forecast = new { date = DateTime.Today, temperature = 42, summary = "Test" };

        // act
        var response = await PutAsync( "api/weatherforecasts/42?api-version=1.0", forecast );

        // assert
        response.StatusCode.Should().Be( OK );
    }

    [Fact]
    public async Task then_patch_should_return_200()
    {
        // arrange
        var forecast = new { summary = "Test" };

        // act
        var response = await PatchAsync( "api/weatherforecasts/42?api-version=1.0", forecast );

        // assert
        response.StatusCode.Should().Be( OK );
    }

    [Fact]
    public async Task then_delete_should_return_204()
    {
        // arrange

        // act
        var response = await DeleteAsync( "api/weatherforecasts/42?api-version=1.0" );

        // assert
        response.StatusCode.Should().Be( NoContent );
    }

    public when_using_builtX2Din_conventions( BasicFixture fixture ) : base( fixture ) { }
}