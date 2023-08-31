// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

/// <content>
/// Provides additional content specific to ASP.NET Web API.
/// </content>
public partial class SunsetPolicyManager
{
    private readonly ApiVersioningOptions options;

    /// <summary>
    /// Initializes a new instance of the <see cref="SunsetPolicyManager"/> class.
    /// </summary>
    /// <param name="options">The associated <see cref="ApiVersioningOptions">API versioning options</see>.</param>
    public SunsetPolicyManager( ApiVersioningOptions options ) => this.options = options;
}