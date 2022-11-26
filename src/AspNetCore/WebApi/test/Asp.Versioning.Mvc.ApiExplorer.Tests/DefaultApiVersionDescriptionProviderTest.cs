// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApiExplorer;

using Microsoft.Extensions.Options;

public class DefaultApiVersionDescriptionProviderTest
{
    [Fact]
    public void api_version_descriptions_should_collate_expected_versions()
    {
        // arrange
        var descriptionProvider = new DefaultApiVersionDescriptionProvider(
            new IApiVersionMetadataCollationProvider[]
            {
                new EndpointApiVersionMetadataCollationProvider( new TestEndpointDataSource() ),
                new ActionApiVersionMetadataCollationProvider( new TestActionDescriptorCollectionProvider() ),
            },
            Mock.Of<ISunsetPolicyManager>(),
            Options.Create( new ApiExplorerOptions() { GroupNameFormat = "'v'VVV" } ) );

        // act
        var descriptions = descriptionProvider.ApiVersionDescriptions;

        // assert
        descriptions.Should().BeEquivalentTo(
            new ApiVersionDescription[]
            {
                new( new ApiVersion( 0, 9 ), "v0.9", true ),
                new( new ApiVersion( 1, 0 ), "v1", false ),
                new( new ApiVersion( 2, 0 ), "v2", false ),
                new( new ApiVersion( 3, 0 ), "v3", false ),
            } );
    }

    [Fact]
    public void api_version_descriptions_should_apply_sunset_policy()
    {
        // arrange
        var expected = new SunsetPolicy();
        var apiVersion = new ApiVersion( 0.9 );
        var policyManager = new Mock<ISunsetPolicyManager>();

        policyManager.Setup( pm => pm.TryGetPolicy( default, apiVersion, out expected ) ).Returns( true );

        var descriptionProvider = new DefaultApiVersionDescriptionProvider(
            new IApiVersionMetadataCollationProvider[]
            {
                new EndpointApiVersionMetadataCollationProvider( new TestEndpointDataSource() ),
                new ActionApiVersionMetadataCollationProvider( new TestActionDescriptorCollectionProvider() ),
            },
            policyManager.Object,
            Options.Create( new ApiExplorerOptions() { GroupNameFormat = "'v'VVV" } ) );

        // act
        var description = descriptionProvider.ApiVersionDescriptions.Single( api => api.GroupName == "v0.9" );

        // assert
        description.SunsetPolicy.Should().BeSameAs( expected );
    }
}