// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable SA1629

namespace Asp.Versioning.OpenApi.Simulators;

/// <summary>
/// Represents a model.
/// </summary>
public class Model
{
    /// <summary>
    /// Gets or sets the user associated with the model.
    /// </summary>
    /// <example>
    /// {
    ///     "userName": "John Doe",
    ///     "email": "john.doe@example.com"
    /// }
    /// </example>
    public User User { get; set; }
}