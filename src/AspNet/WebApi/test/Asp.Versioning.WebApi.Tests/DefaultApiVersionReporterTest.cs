// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using Asp.Versioning.Simulators;
using System.Web.Http;
using System.Web.Http.Controllers;
using static System.Net.HttpStatusCode;

public class DefaultApiVersionReporterTest
{
    [Fact]
    public void report_should_add_expected_headers()
    {
        // arrange
        var sunsetDate = DateTimeOffset.Now;
        var deprecationDate = DateTimeOffset.Now;
        var reporter = new DefaultApiVersionReporter( new TestSunsetPolicyManager( sunsetDate ), new TestDeprecationPolicyManager( deprecationDate ) );
        var configuration = new HttpConfiguration();
        var request = new HttpRequestMessage();
        var response = new HttpResponseMessage( OK ) { RequestMessage = request };
        var apiModel = new ApiVersionModel(
            declaredVersions: new ApiVersion[] { new( 0.9 ), new( 1.0 ), new( 2.0 ) },
            supportedVersions: new ApiVersion[] { new( 1.0 ), new( 2.0 ) },
            deprecatedVersions: new[] { new ApiVersion( 0.9 ) },
            advertisedVersions: Enumerable.Empty<ApiVersion>(),
            deprecatedAdvertisedVersions: Enumerable.Empty<ApiVersion>() );
        var endpointModel = new ApiVersionModel(
            declaredVersions: new ApiVersion[] { new( 1.0 ) },
            supportedVersions: new ApiVersion[] { new( 1.0 ), new( 2.0 ) },
            deprecatedVersions: new[] { new ApiVersion( 0.9 ) },
            advertisedVersions: Enumerable.Empty<ApiVersion>(),
            deprecatedAdvertisedVersions: Enumerable.Empty<ApiVersion>() );
        var metadata = new ApiVersionMetadata( apiModel, endpointModel, "Test" );

        request.SetConfiguration( configuration );
        request.ApiVersionProperties().RequestedApiVersion = new ApiVersion( 1.0 );
        request.Properties["MS_HttpActionDescriptor"] =
            new ReflectedHttpActionDescriptor(
                new HttpControllerDescriptor( configuration, "Test", typeof( TestController ) ),
                typeof( TestController ).GetMethod( nameof( TestController.Get ) ) )
            {
                Properties = { [typeof( ApiVersionMetadata )] = metadata },
            };

        var model = metadata.Map( reporter.Mapping );

        // act
        reporter.Report( response, model );

        // assert
        var headers = response.Headers;

        long unixTimestamp = (int) deprecationDate.Subtract( new DateTime( 1970, 1, 1 ) ).TotalSeconds;

        headers.GetValues( "api-supported-versions" ).Should().Equal( "1.0, 2.0" );
        headers.GetValues( "api-deprecated-versions" ).Should().Equal( "0.9" );
        headers.GetValues( "Sunset" )
               .Single()
               .Should()
               .Be( sunsetDate.ToString( "r" ) );
        headers.GetValues( "Deprecation" )
               .Single()
               .Should()
               .Be( $"@{unixTimestamp}" );
        headers.GetValues( "Link" )
               .Should()
               .BeEquivalentTo( [
                    "<http://docs.api.com/sunset.html>; rel=\"sunset\"",
                    "<http://docs.api.com/deprecation.html>; rel=\"deprecation\"",
                ] );
    }

    private sealed class TestSunsetPolicyManager : ISunsetPolicyManager
    {
        private readonly DateTimeOffset sunsetDate;

        public TestSunsetPolicyManager( DateTimeOffset sunsetDate ) =>
            this.sunsetDate = sunsetDate;

        public bool TryGetPolicy( string name, ApiVersion apiVersion, out SunsetPolicy sunsetPolicy )
        {
            if ( name == "Test" )
            {
                var link = new LinkHeaderValue( new Uri( "http://docs.api.com/sunset.html" ), "sunset" );
                sunsetPolicy = new( sunsetDate, link );
                return true;
            }

            sunsetPolicy = default;
            return false;
        }
    }

    private sealed class TestDeprecationPolicyManager : IDeprecationPolicyManager
    {
        private readonly DateTimeOffset deprecationDate;

        public TestDeprecationPolicyManager( DateTimeOffset deprecationDate ) =>
            this.deprecationDate = deprecationDate;

        public bool TryGetPolicy( string name, ApiVersion apiVersion, out DeprecationPolicy deprecationPolicy )
        {
            if ( name == "Test" )
            {
                var link = new LinkHeaderValue( new Uri( "http://docs.api.com/deprecation.html" ), "deprecation" );
                deprecationPolicy = new( deprecationDate, link );
                return true;
            }

            deprecationPolicy = default;
            return false;
        }
    }
}