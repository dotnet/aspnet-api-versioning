// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using Microsoft.Extensions.Options;

/// <content>
/// Provides additional content specific to ASP.NET Core.
/// </content>
public partial class SunsetPolicyManager
{
    private readonly IOptions<ApiVersioningOptions> options;

    /// <summary>
    /// Initializes a new instance of the <see cref="SunsetPolicyManager"/> class.
    /// </summary>
    /// <param name="options">The associated <see cref="ApiVersioningOptions">API versioning options</see>.</param>
    public SunsetPolicyManager( IOptions<ApiVersioningOptions> options ) => this.options = options;
}