﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApiExplorer;

/// <summary>
/// Represents the description of an API version.
/// </summary>
public class ApiVersionDescription
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersionDescription"/> class.
    /// </summary>
    /// <param name="apiVersion">The described <see cref="ApiVersion">API version</see>.</param>
    /// <param name="groupName">The group name for the API version.</param>
    /// <param name="deprecated">Indicates whether the API version is deprecated.</param>
    /// <param name="sunsetPolicy">The defined <see cref="SunsetPolicy">sunset policy</see>, if any.</param>
    public ApiVersionDescription(
        ApiVersion apiVersion,
        string groupName,
        bool deprecated = false,
        SunsetPolicy? sunsetPolicy = default )
    {
        ApiVersion = apiVersion;
        GroupName = groupName;
        IsDeprecated = deprecated;
        SunsetPolicy = sunsetPolicy;
    }

    /// <summary>
    /// Gets the described API version.
    /// </summary>
    /// <value>The described <see cref="ApiVersion">API version</see>.</value>
    public ApiVersion ApiVersion { get; }

    /// <summary>
    /// Gets the API version group name.
    /// </summary>
    /// <value>The group name for the API version.</value>
    public string GroupName { get; }

    /// <summary>
    /// Gets a value indicating whether the API version is deprecated.
    /// </summary>
    /// <value>True if the API version is deprecated; otherwise, false.</value>
    /// <remarks>An API version will only be described as deprecated when all
    /// all corresponding service implementations are also deprecated. It is
    /// possible that some API versions may be partially deprecated, in which
    /// case this property will return <c>false</c>, but individual actions
    /// may report that they are deprecated.</remarks>
    public bool IsDeprecated { get; }

    /// <summary>
    /// Gets described API sunset policy.
    /// </summary>
    /// <value>The defined sunset policy defined for the API, if any.</value>
    public SunsetPolicy? SunsetPolicy { get; }
}