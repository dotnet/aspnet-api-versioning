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
        var sunsetDate = DateTimeOffset.UtcNow.AddDays( 2 );
        var deprecationDate = DateTimeOffset.UtcNow.AddDays( 1 );
        var reporter = new DefaultApiVersionReporter( new TestSunsetPolicyManager( sunsetDate ), new TestDeprecationPolicyManager( deprecationDate ) );
        var configuration = new HttpConfiguration();
        var request = new HttpRequestMessage();
        var response = new HttpResponseMessage( OK ) { RequestMessage = request };
        var apiModel = new ApiVersionModel(
            declaredVersions: [new( 0.9 ), new( 1.0 ), new( 2.0 )],
            supportedVersions: [new( 1.0 ), new( 2.0 )],
            deprecatedVersions: [new ApiVersion( 0.9 )],
            advertisedVersions: [],
            deprecatedAdvertisedVersions: [] );
        var endpointModel = new ApiVersionModel(
            declaredVersions: [new( 1.0 )],
            supportedVersions: [new( 1.0 ), new( 2.0 )],
            deprecatedVersions: [new ApiVersion( 0.9 )],
            advertisedVersions: [],
            deprecatedAdvertisedVersions: [] );
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

        // This line uses an explicit calculation of the unix timestamp to surface any bugs in the backport of ToUnixTimeSeconds.
        var unixTimestamp = (long) deprecationDate.Subtract( new DateTime( 1970, 1, 1 ) ).TotalSeconds;

        headers.GetValues( "api-supported-versions" ).Should().Equal( "1.0, 2.0" );
        headers.GetValues( "api-deprecated-versions" ).Should().Equal( "0.9" );
        headers.GetValues( "Sunset" )
               .Should()
               .ContainSingle( sunsetDate.ToString( "r" ) );
        headers.GetValues( "Deprecation" )
               .Should()
               .ContainSingle( $"@{unixTimestamp}" );
        headers.GetValues( "Link" )
               .Should()
               .BeEquivalentTo( [
                    "<http://docs.api.com/sunset.html>; rel=\"sunset\"",
                    "<http://docs.api.com/deprecation.html>; rel=\"deprecation\"",
                ] );
    }

    private sealed class TestSunsetPolicyManager : IPolicyManager<SunsetPolicy>
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

    private sealed class TestDeprecationPolicyManager : IPolicyManager<DeprecationPolicy>
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