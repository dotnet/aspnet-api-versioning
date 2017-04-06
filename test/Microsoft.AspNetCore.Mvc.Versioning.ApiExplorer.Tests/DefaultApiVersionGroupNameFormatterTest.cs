namespace Microsoft.AspNetCore.Mvc.ApiExplorer
{
    using FluentAssertions;
    using System;
    using System.Collections.Generic;
    using Xunit;

    public class DefaultApiVersionGroupNameFormatterTest
    {
        [Theory]
        [MemberData( nameof( GroupNameData ) )]
        public void get_group_name_should_return_expected_text( ApiVersion version, string groupName )
        {
            // arrange
            var formatter = new DefaultApiVersionGroupNameFormatter();

            // act
            var result = formatter.GetGroupName( version );

            // assert
            result.Should().Be( groupName );
        }

        public static IEnumerable<object[]> GroupNameData
        {
            get
            {
                yield return new object[] { new ApiVersion( 1, 0 ), "v1" };
                yield return new object[] { new ApiVersion( 1, 0, "RC" ), "v1-RC" };
                yield return new object[] { new ApiVersion( 1, 1 ), "v1.1" };
                yield return new object[] { new ApiVersion( 1, 1, "Beta" ), "v1.1-Beta" };
                yield return new object[] { new ApiVersion( 0, 1 ), "v0.1" };
                yield return new object[] { new ApiVersion( 0, 1, "Alpha" ), "v0.1-Alpha" };
                yield return new object[] { new ApiVersion( new DateTime( 2017, 4, 1 ) ), "2017-04-01" };
                yield return new object[] { new ApiVersion( new DateTime( 2017, 4, 1 ), "Beta" ), "2017-04-01-Beta" };
                yield return new object[] { new ApiVersion( new DateTime( 2017, 4, 1 ), 1, 0 ), "2017-04-01-1" };
                yield return new object[] { new ApiVersion( new DateTime( 2017, 4, 1 ), 1, 0, "RC" ), "2017-04-01-1-RC" };
                yield return new object[] { new ApiVersion( new DateTime( 2017, 4, 1 ), 1, 1 ), "2017-04-01-1.1" };
                yield return new object[] { new ApiVersion( new DateTime( 2017, 4, 1 ), 1, 1, "Beta" ), "2017-04-01-1.1-Beta" };
                yield return new object[] { new ApiVersion( new DateTime( 2017, 4, 1 ), 0, 1 ), "2017-04-01-0.1" };
                yield return new object[] { new ApiVersion( new DateTime( 2017, 4, 1 ), 0, 1, "Alpha" ), "2017-04-01-0.1-Alpha" };
            }
        }
    }
}