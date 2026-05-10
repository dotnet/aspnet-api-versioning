// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

/// <summary>
/// Defines the behavior of convention builder that builds introduced API versions.
/// </summary>
public interface IIntroducedInApiVersionConventionBuilder : IMapToApiVersionConventionBuilder
{
    /// <summary>
    /// Indicates that the configured controller action was introduced in the specified API version.
    /// </summary>
    /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> the action was introduced in.</param>
    /// <param name="statusCode">The HTTP status code for earlier API versions.</param>
    void IntroducedInApiVersion( ApiVersion apiVersion, int statusCode );
}