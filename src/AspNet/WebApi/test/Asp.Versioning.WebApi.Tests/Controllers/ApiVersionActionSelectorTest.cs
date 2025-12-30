// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Controllers;

using System.Collections.ObjectModel;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using static System.Net.Http.HttpMethod;

public class ApiVersionActionSelectorTest
{
    [Theory]
    [MemberData( nameof( SelectActionVersionData ) )]
    public void select_action_version_should_return_expected_result( string version, int index )
    {
        // arrange
        var candidates = NewCandidates();
        var expectedAction = candidates[index];
        var configuration = new HttpConfiguration();
        var request = new HttpRequestMessage( Get, "http://localhost/api/test?api-version=" + version );
        var context = new HttpControllerContext() { Request = request };
        var selector = new TestApiVersionActionSelector();

        configuration.AddApiVersioning();
        request.SetConfiguration( configuration );

        // act
        var selectedAction = selector.InvokeSelectActionVersion( context, candidates );

        // assert
        selectedAction.Should().Be( expectedAction );
    }

    private static HttpActionDescriptor[] NewCandidates() =>
    [
        CreateActionDescriptor( "1.0" ),
        CreateActionDescriptor( "2.0" ),
        CreateActionDescriptor( "3.0" ),
    ];

    public static TheoryData<string, int> SelectActionVersionData => new()
    {
        { "1.0", 0 },
        { "2.0", 1 },
        { "3.0", 2 },
    };

    private static HttpActionDescriptor CreateActionDescriptor( string version )
    {
        var configuration = new HttpConfiguration();
        var controllerType = typeof( IHttpController );
        var controllerDescriptor = new Mock<HttpControllerDescriptor>( configuration, "Test", controllerType ) { CallBase = true };
        var actionDescriptor = new Mock<HttpActionDescriptor>() { CallBase = true };
        var attribute = new ApiVersionAttribute( version );
        var metadata = new ApiVersionMetadata( ApiVersionModel.Empty, new ApiVersionModel( attribute.Versions[0] ) );

        controllerDescriptor.Setup( cd => cd.GetCustomAttributes<IApiVersionProvider>( It.IsAny<bool>() ) )
                            .Returns( () => [] );

        actionDescriptor.Setup( ad => ad.GetCustomAttributes<IApiVersionProvider>( It.IsAny<bool>() ) )
                        .Returns( () => [attribute] );

        var newActionDescriptor = actionDescriptor.Object;

        newActionDescriptor.ControllerDescriptor = controllerDescriptor.Object;
        newActionDescriptor.Properties[typeof( ApiVersionMetadata )] = metadata;

        return newActionDescriptor;
    }

    private sealed class TestApiVersionActionSelector : ApiVersionActionSelector
    {
        internal HttpActionDescriptor InvokeSelectActionVersion(
            HttpControllerContext controllerContext,
            IReadOnlyList<HttpActionDescriptor> candidateActions ) => SelectActionVersion( controllerContext, candidateActions );
    }
}