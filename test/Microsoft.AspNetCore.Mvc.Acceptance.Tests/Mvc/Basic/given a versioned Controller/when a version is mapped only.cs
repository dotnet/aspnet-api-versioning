namespace given_a_versioned_Controller
{
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Basic;
    using System;
    using System.Threading.Tasks;
    using Xunit;
    using static System.Net.HttpStatusCode;

    [Trait( "Routing", "Classic" )]
    [Collection( nameof( BasicCollection ) )]
    public class when_a_version_is_mapped_only : AcceptanceTest
    {
        [Fact]
        public async Task then_get_should_return_400()
        {
            // arrange
            var requestUrl = "api/v42/helloworld/unreachable";

            // act
            var response = await GetAsync( requestUrl );

            // assert
            response.StatusCode.Should().Be( BadRequest );
        }

        public when_a_version_is_mapped_only( BasicFixture fixture ) : base( fixture ) { }
    }

    [Trait( "Routing", "Endpoint" )]
    [Collection( nameof( BasicEndpointCollection ) )]
    public class when_a_version_is_mapped_only_ : when_a_version_is_mapped_only
    {
        public when_a_version_is_mapped_only_( BasicEndpointFixture fixture ) : base( fixture ) { }
    }
}