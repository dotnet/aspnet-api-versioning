// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OpenApi.Transformers;

using Asp.Versioning.OpenApi.Simulators;
using System.Text.Json.Nodes;

public class XmlCommentsTest
{
    [Fact]
    public void summary_should_be_retrieved_for_minimal_api()
    {
        // arrange
        var comments = XmlComments.FromFile( FilePath.XmlCommentFile );
        var method = typeof( MinimalApi ).GetMethod( nameof( MinimalApi.Get ) );

        // act
        var summary = comments.GetSummary( method );

        // assert
        summary.Should().Be( "Test" );
    }

    [Fact]
    public void description_should_be_retrieved_for_minimal_api()
    {
        // arrange
        var comments = XmlComments.FromFile( FilePath.XmlCommentFile );
        var method = typeof( MinimalApi ).GetMethod( nameof( MinimalApi.Get ) );

        // act
        var description = comments.GetDescription( method );

        // assert
        description.Should().Be( "A test API." );
    }

    [Fact]
    public void parameter_description_should_be_retrieved_for_minimal_api()
    {
        // arrange
        var comments = XmlComments.FromFile( FilePath.XmlCommentFile );
        var method = typeof( MinimalApi ).GetMethod( nameof( MinimalApi.Get ) );

        // act
        var description = comments.GetParameterDescription( method, "id" );

        // assert
        description.Should().Be( "A test parameter." );
    }

    [Fact]
    public void response_description_should_be_retrieved_for_minimal_api()
    {
        // arrange
        var comments = XmlComments.FromFile( FilePath.XmlCommentFile );
        var method = typeof( MinimalApi ).GetMethod( nameof( MinimalApi.Get ) );

        // act
        var description = comments.GetResponseDescription( method, 200 );

        // assert
        description.Should().Be( "Pass" );
    }

    [Fact]
    public void parameter_example_should_be_retrieved_for_minimal_api()
    {
        // arrange
        var comments = XmlComments.FromFile( FilePath.XmlCommentFile );
        var method = typeof( MinimalApi ).GetMethod( nameof( MinimalApi.Get ) );

        // act
        var example = comments.GetParameterExample( method, "id" );

        // assert
        example.Should().Be( "42" );
    }

    [Fact]
    public void summary_should_be_retrieved_for_controller()
    {
        // arrange
        var comments = XmlComments.FromFile( FilePath.XmlCommentFile );
        var method = typeof( TestController ).GetMethod( nameof( TestController.Get ) );

        // act
        var summary = comments.GetSummary( method );

        // assert
        summary.Should().Be( "Test" );
    }

    [Fact]
    public void description_should_be_retrieved_for_controller()
    {
        // arrange
        var comments = XmlComments.FromFile( FilePath.XmlCommentFile );
        var method = typeof( TestController ).GetMethod( nameof( TestController.Get ) );

        // act
        var description = comments.GetDescription( method );

        // assert
        description.Should().Be( "A test API." );
    }

    [Fact]
    public void parameter_description_should_be_retrieved_for_controller()
    {
        // arrange
        var comments = XmlComments.FromFile( FilePath.XmlCommentFile );
        var method = typeof( TestController ).GetMethod( nameof( TestController.Get ) );

        // act
        var description = comments.GetParameterDescription( method, "id" );

        // assert
        description.Should().Be( "A test parameter." );
    }

    [Fact]
    public void response_description_should_be_retrieved_for_controller()
    {
        // arrange
        var comments = XmlComments.FromFile( FilePath.XmlCommentFile );
        var method = typeof( TestController ).GetMethod( nameof( TestController.Get ) );

        // act
        var description = comments.GetResponseDescription( method, 400 );

        // assert
        description.Should().Be( "Fail" );
    }

    [Fact]
    public void example_parameter_should_be_retrieved_for_controller()
    {
        // arrange
        var comments = XmlComments.FromFile( FilePath.XmlCommentFile );
        var method = typeof( TestController ).GetMethod( nameof( TestController.Get ) );

        // act
        var example = comments.GetParameterExample( method, "id" );

        // assert
        example.Should().Be( "42" );
    }

    [Fact]
    public void example_property_should_be_retrieved_from_model()
    {
        // arrange
        var comments = XmlComments.FromFile( FilePath.XmlCommentFile );
        var property = typeof( Model ).GetProperty( nameof( Model.User ) );
        var expected = JsonNode.Parse( """{"userName":"John Doe","email":"john.doe@example.com"}""" );

        // act
        var actual = JsonNode.Parse( comments.GetExample( property ) );

        // assert
        JsonNode.DeepEquals( expected, actual ).Should().BeTrue();
    }

    [Fact]
    public void example_property_should_be_retrieved_from_nested_model()
    {
        // arrange
        var comments = XmlComments.FromFile( FilePath.XmlCommentFile );
        var property = typeof( User ).GetProperty( nameof( User.Email ) );

        // act
        var example = comments.GetExample( property );

        // assert
        example.Should().Be( "user@example.com" );
    }
}