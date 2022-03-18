// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using Asp.Versioning.Routing;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Routing;
using static ApiVersionParameterLocation;
using static System.Net.Http.HttpMethod;

public class UrlSegmentApiVersionReaderTest
{
    [Fact]
    public void read_should_retrieve_version_from_url()
    {
        // arrange
        var requestedVersion = "2";
        var configuration = NewConfiguration();
        var request = new HttpRequestMessage( Get, $"http://localhost/api/v{requestedVersion}/test" );
        var reader = new UrlSegmentApiVersionReader();

        configuration.EnsureInitialized();
        request.SetConfiguration( configuration );

        var routeData = configuration.Routes.GetRouteData( request );

        request.SetRouteData( routeData );

        // act
        var versions = reader.Read( request );

        // assert
        versions.Single().Should().Be( requestedVersion );
    }

    [Fact]
    public void add_parameters_should_add_parameter_for_url_segment()
    {
        // arrange
        var reader = new UrlSegmentApiVersionReader();
        var context = new Mock<IApiVersionParameterDescriptionContext>();

        context.Setup( c => c.AddParameter( It.IsAny<string>(), It.IsAny<ApiVersionParameterLocation>() ) );

        // act
        reader.AddParameters( context.Object );

        // assert
        context.Verify( c => c.AddParameter( string.Empty, Path ), Times.Once() );
    }

    private static HttpConfiguration NewConfiguration()
    {
        var configuration = new HttpConfiguration();
        var constraintResolver = new DefaultInlineConstraintResolver()
        {
            ConstraintMap = { ["apiVersion"] = typeof( ApiVersionRouteConstraint ) },
        };

        configuration.MapHttpAttributeRoutes( constraintResolver );

        return configuration;
    }
}