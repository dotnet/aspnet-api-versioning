// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using Asp.Versioning.Conventions;

/// <content>
/// Provides additional implementation specific to ASP.NET Web API.
/// </content>
public partial class ApiVersioningOptions
{
    private IApiVersionConventionBuilder? conventions;

    /// <summary>
    /// Gets or sets the builder used to define API version conventions.
    /// </summary>
    /// <value>An <see cref="IApiVersionConventionBuilder">API version convention builder</see>.</value>
    public IApiVersionConventionBuilder Conventions
    {
        get => conventions ??= new ApiVersionConventionBuilder();
        set => conventions = value;
    }
}