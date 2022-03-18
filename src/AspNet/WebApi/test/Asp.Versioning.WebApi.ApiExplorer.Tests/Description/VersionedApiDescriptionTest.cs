// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Description;

using System.Web.Http.Description;

public class VersionedApiDescriptionTest
{
    [Fact]
    public void shadowed_ResponseDescription_property_should_set_internal_value()
    {
        // arrange
        var apiDescription = new VersionedApiDescription();
        var responseDescription = new ResponseDescription() { Documentation = "Test" };

        // act
        apiDescription.ResponseDescription = responseDescription;

        // assert
        apiDescription.ResponseDescription.Should().BeSameAs( responseDescription );
    }
}