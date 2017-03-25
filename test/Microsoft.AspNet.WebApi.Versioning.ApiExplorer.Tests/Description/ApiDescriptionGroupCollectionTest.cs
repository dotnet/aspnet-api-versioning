namespace Microsoft.Web.Http.Description
{
    using FluentAssertions;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Xunit;

    public class ApiDescriptionGroupCollectionTest
    {
        [Fact]
        public void versions_should_return_sorted_values()
        {
            // arrange
            var collection = new ApiDescriptionGroupCollection()
            {
                new ApiDescriptionGroup( new ApiVersion( 3, 0 ) ),
                new ApiDescriptionGroup( new ApiVersion( 1, 0 ) ),
                new ApiDescriptionGroup( new ApiVersion( 2, 0 ) )
            };

            // act
            var versions = collection.Versions;

            // assert
            versions.Should().BeEquivalentTo( new ApiVersion( 1, 0 ), new ApiVersion( 2, 0 ), new ApiVersion( 3, 0 ) );
        }

        [Fact]
        public void flatten_should_return_denormalized_api_descriptions_in_order()
        {
            // arrange
            var collection = new ApiDescriptionGroupCollection()
            {
                new ApiDescriptionGroup( new ApiVersion( 3, 0 ) )
                {
                    ApiDescriptions =
                    {
                        new VersionedApiDescription() { Version = new ApiVersion( 3, 0 ), RelativePath = "api/values" },
                        new VersionedApiDescription() { Version = new ApiVersion( 3, 0 ), RelativePath = "api/orders" }
                    }
                },
                new ApiDescriptionGroup( new ApiVersion( 1, 0 ) )
                {
                    ApiDescriptions =
                    {
                        new VersionedApiDescription() { Version = new ApiVersion( 1, 0 ), RelativePath = "api/people" },
                        new VersionedApiDescription() { Version = new ApiVersion( 1, 0 ), RelativePath = "api/orders" }
                    }
                },
                new ApiDescriptionGroup( new ApiVersion( 2, 0 ) )
                {
                    ApiDescriptions =
                    {
                        new VersionedApiDescription() { Version = new ApiVersion( 2, 0 ), RelativePath = "api/values" },
                        new VersionedApiDescription() { Version = new ApiVersion( 2, 0 ), RelativePath = "api/people" },
                        new VersionedApiDescription() { Version = new ApiVersion( 2, 0 ), RelativePath = "api/orders" }
                    }
                }
            };

            // act
            var descriptions = collection.Flatten().Cast<VersionedApiDescription>();

            // assert
            descriptions.ShouldBeEquivalentTo(
                new Collection<VersionedApiDescription>()
                {
                    new VersionedApiDescription() { Version = new ApiVersion( 1, 0 ), RelativePath = "api/people" },
                    new VersionedApiDescription() { Version = new ApiVersion( 1, 0 ), RelativePath = "api/orders" },
                    new VersionedApiDescription() { Version = new ApiVersion( 2, 0 ), RelativePath = "api/values" },
                    new VersionedApiDescription() { Version = new ApiVersion( 2, 0 ), RelativePath = "api/people" },
                    new VersionedApiDescription() { Version = new ApiVersion( 2, 0 ), RelativePath = "api/orders" },
                    new VersionedApiDescription() { Version = new ApiVersion( 3, 0 ), RelativePath = "api/values" },
                    new VersionedApiDescription() { Version = new ApiVersion( 3, 0 ), RelativePath = "api/orders" }
                } );
        }
    }
}