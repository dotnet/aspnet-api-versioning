// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Primitives;

public class DefaultApiVersionReporterTest
{
    [Fact]
    public void report_should_add_expected_headers()
    {
        // arrange
        var sunsetDate = DateTimeOffset.UtcNow.AddDays( 2 );
        var deprecationDate = DateTimeOffset.UtcNow.AddDays( 1 );
        var reporter = new DefaultApiVersionReporter( new TestSunsetPolicyManager( sunsetDate ), new TestDeprecationPolicyManager( deprecationDate ) );
        var httpContext = new Mock<HttpContext>();
        var features = new Mock<IFeatureCollection>();
        var query = new Mock<IQueryCollection>();
        var request = new Mock<HttpRequest>();
        var response = new Mock<HttpResponse>();
        var headers = new HeaderDictionary()
        {
            ["Content-Type"] = "application/json",
        };
        var serviceProvider = new Mock<IServiceProvider>();
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
        var endpoint = new Endpoint( c => Task.CompletedTask, new( metadata ), default );
        var endpoints = new Mock<IEndpointFeature>();

        endpoints.SetupProperty( e => e.Endpoint, endpoint );
        features.Setup( f => f.Get<IApiVersioningFeature>() ).Returns( () => new ApiVersioningFeature( httpContext.Object ) );
        features.Setup( f => f.Get<IEndpointFeature>() ).Returns( endpoints.Object );
        query.SetupGet( q => q["api-version"] ).Returns( new StringValues( "42.0" ) );
        request.SetupGet( r => r.Query ).Returns( query.Object );
        response.SetupProperty( r => r.StatusCode, 200 );
        response.SetupGet( r => r.Headers ).Returns( headers );
        response.SetupGet( r => r.HttpContext ).Returns( () => httpContext.Object );
        serviceProvider.Setup( sp => sp.GetService( typeof( IApiVersionParser ) ) ).Returns( ApiVersionParser.Default );
        serviceProvider.Setup( sp => sp.GetService( typeof( IApiVersionReader ) ) ).Returns( new QueryStringApiVersionReader() );
        httpContext.SetupGet( c => c.Features ).Returns( features.Object );
        httpContext.SetupGet( c => c.Request ).Returns( request.Object );
        httpContext.SetupProperty( c => c.RequestServices, serviceProvider.Object );

        var model = metadata.Map( reporter.Mapping );

        // act
        reporter.Report( response.Object, model );

        // assert
        var unixTimestamp = deprecationDate.ToUnixTimeSeconds();

        headers["api-supported-versions"].Should().Equal( "1.0, 2.0" );
        headers["api-deprecated-versions"].Should().Equal( "0.9" );
        headers["Sunset"]
            .Should()
            .ContainSingle( sunsetDate.ToString( "r" ) );
        headers["Deprecation"]
            .Should()
            .ContainSingle( $"@{unixTimestamp}" );
        headers["Link"]
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