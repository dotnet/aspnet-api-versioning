// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using Asp.Versioning.Conventions;

/// <summary>
/// Represents the API versioning options specific to ASP.NET Core MVC.
/// </summary>
public class MvcApiVersioningOptions
{
    private IApiVersionConventionBuilder? conventions;

    /// <summary>
    /// Gets or sets the builder used to define API version conventions.
    /// </summary>
    /// <value>An <see cref="IApiVersionConventionBuilder">API version convention builder</see>.</value>
    [CLSCompliant( false )]
    public IApiVersionConventionBuilder Conventions
    {
        get => conventions ??= new ApiVersionConventionBuilder();
        set => conventions = value;
    }
}