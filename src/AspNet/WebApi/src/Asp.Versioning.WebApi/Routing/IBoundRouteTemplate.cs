// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using System.Web.Http.Routing;

/// <summary>
/// Defines the behavior of a bound route template.
/// </summary>
public interface IBoundRouteTemplate
{
    /// <summary>
    /// Gets or sets the build template.
    /// </summary>
    /// <value>The bound template.</value>
    string BoundTemplate { get; set; }

    /// <summary>
    /// Gets or sets the template parameter values.
    /// </summary>
    /// <value>The template <see cref="HttpRouteValueDictionary">route value dictionary</see>.</value>
    HttpRouteValueDictionary Values { get; set; }
}