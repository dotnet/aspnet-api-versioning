// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApiExplorer;

/// <summary>
/// Defines the behavior of a member filter.
/// </summary>
/// <typeparam name="T">The type of member.</typeparam>
public interface IMemberFilter<T>
{
    /// <summary>
    /// Determines whether the provide member is visible to the specified API version.
    /// </summary>
    /// <param name="member">The member to evaluate.</param>
    /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> to compare against.</param>
    /// <returns>True if the member is visible in the specified <paramref name="apiVersion">API version</paramref>;
    /// otherwise, false.</returns>
    bool IsVisible( T member, ApiVersion apiVersion );
}