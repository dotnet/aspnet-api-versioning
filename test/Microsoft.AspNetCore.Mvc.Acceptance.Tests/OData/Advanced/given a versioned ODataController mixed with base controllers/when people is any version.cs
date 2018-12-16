namespace given_a_versioned_ODataController_mixed_with_base_controllers
{
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.OData;
    using Microsoft.AspNetCore.OData.Advanced;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Xunit;
    using static System.Net.HttpStatusCode;

    [Collection( nameof( AdvancedODataCollection ) )]
    public class when_people_is_any_version : ODataAcceptanceTest
    {
        [Fact]
        public async Task then_patch_should_return_400_for_an_unsupported_version()
        {
            // arrange
            var person = new { lastName = "Me" };

            // act
            var response = await PatchAsync( $"api/people(42)?api-version=4.0", person );
            var content = await response.Content.ReadAsAsync<OneApiErrorResponse>();

            // assert
            response.StatusCode.Should().Be( BadRequest );
            content.Error.Code.Should().Be( "UnsupportedApiVersion" );
        }

        public when_people_is_any_version( AdvancedFixture fixture ) : base( fixture ) { }
    }
}