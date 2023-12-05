// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Description;

using System.Collections.ObjectModel;
using System.Diagnostics;

/// <summary>
/// Represents a group of versioned API descriptions.
/// </summary>
[DebuggerDisplay( "ApiVersion = {ApiVersion}, Count = {ApiDescriptions.Count}" )]
public class ApiDescriptionGroup
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApiDescriptionGroup"/> class.
    /// </summary>
    /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> associated with the group.</param>
    /// <param name="name">The name of the API description group.</param>
    public ApiDescriptionGroup( ApiVersion apiVersion, string name )
    {
        ApiVersion = apiVersion;
        Name = name;
    }

    /// <summary>
    /// Gets the version associated with the group of APIs.
    /// </summary>
    /// <value>An <see cref="ApiVersion">API version</see>.</value>
    public ApiVersion ApiVersion { get; }

    /// <summary>
    /// Gets the name of the API description group.
    /// </summary>
    /// <value>The API version description group name.</value>
    public string Name { get; }

    /// <summary>
    /// Gets a value indicating whether API version is deprecated for all described APIs in the group.
    /// </summary>
    /// <value>True if all APIs in the group are deprecated; otherwise, false.</value>
    /// <remarks>An API version will only be described as deprecated when all
    /// all corresponding service implementations are also deprecated. It is
    /// possible that some API versions may be partially deprecated, in which
    /// case this property will return <c>false</c>, but individual actions
    /// may report that they are deprecated.</remarks>
    public virtual bool IsDeprecated => ApiDescriptions.All( d => d.IsDeprecated );

    /// <summary>
    /// Gets or sets described API sunset policy.
    /// </summary>
    /// <value>The defined sunset policy defined for the API, if any.</value>
    public SunsetPolicy? SunsetPolicy { get; set; }

    /// <summary>
    /// Gets a collection of API descriptions for the current version.
    /// </summary>
    /// <value>A <see cref="Collection{T}">collection</see> of
    /// <see cref="VersionedApiDescription">versioned API descriptions</see>.</value>
    public virtual Collection<VersionedApiDescription> ApiDescriptions { get; } = [];
}