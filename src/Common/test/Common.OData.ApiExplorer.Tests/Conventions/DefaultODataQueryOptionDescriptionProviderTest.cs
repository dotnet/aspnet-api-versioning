// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

#if NETFRAMEWORK
using Microsoft.AspNet.OData.Query;
using static Microsoft.AspNet.OData.Query.AllowedArithmeticOperators;
using static Microsoft.AspNet.OData.Query.AllowedFunctions;
using static Microsoft.AspNet.OData.Query.AllowedLogicalOperators;
using static Microsoft.AspNet.OData.Query.AllowedQueryOptions;
#else
using Microsoft.AspNetCore.OData.Query;
using static Microsoft.AspNetCore.OData.Query.AllowedArithmeticOperators;
using static Microsoft.AspNetCore.OData.Query.AllowedFunctions;
using static Microsoft.AspNetCore.OData.Query.AllowedLogicalOperators;
using static Microsoft.AspNetCore.OData.Query.AllowedQueryOptions;
#endif

public class DefaultODataQueryOptionDescriptionProviderTest
{
    [Fact]
    public void describe_should_not_allow_invalid_query_option()
    {
        // arrange
        var provider = new DefaultODataQueryOptionDescriptionProvider();
        var message = FormatMessage( "Only a single, valid query option may be specified.", "queryOption" );

        // act
        Action describe = () => provider.Describe( AllowedQueryOptions.None, new( new() ) );

        // assert
        describe.Should().Throw<ArgumentException>().And.Message.Should().Be( message );
    }

    [Fact]
    public void describe_should_not_allow_multiple_query_options()
    {
        // arrange
        var provider = new DefaultODataQueryOptionDescriptionProvider();
        var message = FormatMessage( "Only a single, valid query option may be specified.", "queryOption" );

        // act
        Action describe = () => provider.Describe( Supported, new( new() ) );

        // assert
        describe.Should().Throw<ArgumentException>().And.Message.Should().Be( message );
    }

    [Fact]
    public void describe_should_not_allow_unsupported_query_option()
    {
        // arrange
        var provider = new DefaultODataQueryOptionDescriptionProvider();
        var message = FormatMessage( "The query option $skiptoken is not supported.", "queryOption" );

        // act
        Action describe = () => provider.Describe( SkipToken, new( new() ) );

        // assert
        describe.Should().Throw<ArgumentException>().And.Message.Should().Be( message );
    }

    [Fact]
    public void describe_should_return_description_for_count()
    {
        // arrange
        var provider = new DefaultODataQueryOptionDescriptionProvider();

        // act
        var description = provider.Describe( Count, new( new() ) );

        // assert
        description.Should().Be( "Indicates whether the total count of items within a collection are returned in the result." );
    }

    [Theory]
    [InlineData( null, "Excludes the specified number of items of the queried collection from the result." )]
    [InlineData( 42, "Excludes the specified number of items of the queried collection from the result. The maximum value is 42." )]
    public void describe_should_return_description_for_skip( int? maxSkip, string expected )
    {
        // arrange
        var provider = new DefaultODataQueryOptionDescriptionProvider();

        // act
        var description = provider.Describe( Skip, new( new() ) { MaxSkip = maxSkip } );

        // assert
        description.Should().Be( expected );
    }

    [Theory]
    [InlineData( null, "Limits the number of items returned from a collection." )]
    [InlineData( 42, "Limits the number of items returned from a collection. The maximum value is 42." )]
    public void describe_should_return_description_for_top( int? maxTop, string expected )
    {
        // arrange
        var provider = new DefaultODataQueryOptionDescriptionProvider();

        // act
        var description = provider.Describe( Top, new( new() ) { MaxTop = maxTop } );

        // assert
        description.Should().Be( expected );
    }

    [Theory]
    [InlineData( 0, new string[0], "Specifies the order in which items are returned." )]
    [InlineData( 2, new string[0], "Specifies the order in which items are returned. The maximum number of expressions is 2." )]
    [InlineData( 3, new[] { "firstName", "lastName" }, "Specifies the order in which items are returned. The maximum number of expressions is 3. The allowed properties are: firstName, lastName." )]
    public void describe_should_return_description_for_orderby( int maxNodeCount, string[] properties, string expected )
    {
        // arrange
        var provider = new DefaultODataQueryOptionDescriptionProvider();
        var context = new ODataQueryOptionDescriptionContext( new() ) { MaxOrderByNodeCount = maxNodeCount };

        for ( var i = 0; i < properties.Length; i++ )
        {
            context.AllowedOrderByProperties.Add( properties[i] );
        }

        // act
        var description = provider.Describe( OrderBy, context );

        // assert
        description.Should().Be( expected );
    }

    [Theory]
    [InlineData( new string[0], "Limits the properties returned in the result." )]
    [InlineData( new[] { "id", "firstName", "lastName" }, "Limits the properties returned in the result. The allowed properties are: id, firstName, lastName." )]
    public void describe_should_return_description_for_select( string[] properties, string expected )
    {
        // arrange
        var provider = new DefaultODataQueryOptionDescriptionProvider();
        var context = new ODataQueryOptionDescriptionContext( new() );

        for ( var i = 0; i < properties.Length; i++ )
        {
            context.AllowedSelectProperties.Add( properties[i] );
        }

        // act
        var description = provider.Describe( Select, context );

        // assert
        description.Should().Be( expected );
    }

    [Theory]
    [InlineData( 0, new string[0], "Indicates the related entities to be represented inline." )]
    [InlineData( 2, new string[0], "Indicates the related entities to be represented inline. The maximum depth is 2." )]
    [InlineData( 3, new[] { "address", "lineItems" }, "Indicates the related entities to be represented inline. The maximum depth is 3. The allowed properties are: address, lineItems." )]
    public void describe_should_return_description_for_expand( int maxDepth, string[] properties, string expected )
    {
        // arrange
        var provider = new DefaultODataQueryOptionDescriptionProvider();
        var context = new ODataQueryOptionDescriptionContext( new() ) { MaxExpansionDepth = maxDepth };

        for ( var i = 0; i < properties.Length; i++ )
        {
            context.AllowedExpandProperties.Add( properties[i] );
        }

        // act
        var description = provider.Describe( Expand, context );

        // assert
        description.Should().Be( expected );
    }

    [Theory]
    [MemberData( nameof( FilterDescriptionData ) )]
    public void describe_should_return_description_for_filter(
        int maxNodeCount,
        string[] properties,
        AllowedLogicalOperators logicalOperators,
        AllowedArithmeticOperators arithmeticOperators,
        AllowedFunctions functions,
        string expected )
    {
        // arrange
        var provider = new DefaultODataQueryOptionDescriptionProvider();
        var context = new ODataQueryOptionDescriptionContext( new() )
        {
            MaxNodeCount = maxNodeCount,
            AllowedArithmeticOperators = arithmeticOperators,
            AllowedLogicalOperators = logicalOperators,
            AllowedFunctions = functions,
        };

        for ( var i = 0; i < properties.Length; i++ )
        {
            context.AllowedFilterProperties.Add( properties[i] );
        }

        // act
        var description = provider.Describe( Filter, context );

        // assert
        description.Should().Be( expected );
    }

    public static IEnumerable<object[]> FilterDescriptionData
    {
        get
        {
            yield return new object[]
            {
                0,
                Array.Empty<string>(),
                AllowedLogicalOperators.None,
                AllowedArithmeticOperators.None,
                AllowedFunctions.None,
                "Restricts the set of items returned.",
            };

            yield return new object[]
            {
                2,
                Array.Empty<string>(),
                AllowedLogicalOperators.None,
                AllowedArithmeticOperators.None,
                AllowedFunctions.None,
                "Restricts the set of items returned. The maximum number of expressions is 2.",
            };

            yield return new object[]
            {
                3,
                Array.Empty<string>(),
                AllowedLogicalOperators.All,
                Add | Subtract,
                AllowedFunctions.None,
                "Restricts the set of items returned. The maximum number of expressions is 3. The allowed arithmetic operators are: add, sub.",
            };

            yield return new object[]
            {
                5,
                new[] { "name", "price", "quantity" },
                And,
                AllowedArithmeticOperators.All,
                Contains | StartsWith | EndsWith,
                "Restricts the set of items returned. The maximum number of expressions is 5. The allowed logical operators are: and. The allowed functions are: startswith, endswith, contains. The allowed properties are: name, price, quantity.",
            };

            yield return new object[]
            {
                0,
                new[] { "category", "price", "quantity" },
                AllowedLogicalOperators.All,
                AllowedArithmeticOperators.All,
                AllFunctions,
                "Restricts the set of items returned. The allowed properties are: category, price, quantity.",
            };
        }
    }

    private static string FormatMessage( string message, string paramName ) =>
        new ArgumentException( message, paramName ).Message;
}