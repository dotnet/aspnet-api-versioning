// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OpenApi.Transformers;

using Asp.Versioning.OpenApi.Simulators;

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
}