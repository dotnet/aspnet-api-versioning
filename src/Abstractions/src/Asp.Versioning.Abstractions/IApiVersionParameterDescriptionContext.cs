// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

/// <summary>
/// Defines the behavior of an object that contains API version parameter descriptions.
/// </summary>
public interface IApiVersionParameterDescriptionContext
{
    /// <summary>
    /// Adds an API version parameter with the specified name, from the specified location.
    /// </summary>
    /// <param name="name">The name of the parameter.</param>
    /// <param name="location">One of the <see cref="ApiVersionParameterLocation"/> values.</param>
    void AddParameter( string name, ApiVersionParameterLocation location );
}