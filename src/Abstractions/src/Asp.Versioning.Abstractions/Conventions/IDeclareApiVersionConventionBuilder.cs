// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

/// <summary>
/// Defines the behavior of convention builder that builds declared API versions.
/// </summary>
public interface IDeclareApiVersionConventionBuilder
{
    /// <summary>
    /// Indicates that the controller is API version-neutral.
    /// </summary>
    void IsApiVersionNeutral();

    /// <summary>
    /// Indicates that the specified API version is supported by the configured controller.
    /// </summary>
    /// <param name="apiVersion">The supported <see cref="ApiVersion">API version</see> implemented by the controller.</param>
    void HasApiVersion( ApiVersion apiVersion );

    /// <summary>
    /// Indicates that the specified API version is deprecated by the configured controller.
    /// </summary>
    /// <param name="apiVersion">The deprecated <see cref="ApiVersion">API version</see> implemented by the controller.</param>
    void HasDeprecatedApiVersion( ApiVersion apiVersion );

    /// <summary>
    /// Indicates that the specified API version is advertised by the configured controller.
    /// </summary>
    /// <param name="apiVersion">The advertised <see cref="ApiVersion">API version</see> not directly implemented by the controller.</param>
    void AdvertisesApiVersion( ApiVersion apiVersion );

    /// <summary>
    /// Indicates that the specified API version is advertised and deprecated by the configured controller.
    /// </summary>
    /// <param name="apiVersion">The advertised, but deprecated <see cref="ApiVersion">API version</see> not directly implemented by the controller.</param>
    void AdvertisesDeprecatedApiVersion( ApiVersion apiVersion );
}