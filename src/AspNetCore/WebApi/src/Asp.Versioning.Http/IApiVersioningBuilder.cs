// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Defines the behavior for configuring API versioning.
/// </summary>
public partial interface IApiVersioningBuilder
{
    /// <summary>
    /// Gets the services used when configuring API versioning.
    /// </summary>
    /// <value>The <see cref="IServiceCollection">service collection</see> used
    /// by API versioning.</value>
    IServiceCollection Services { get; }
}