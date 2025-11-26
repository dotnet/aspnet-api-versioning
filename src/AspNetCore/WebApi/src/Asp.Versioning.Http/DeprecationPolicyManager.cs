// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using Microsoft.Extensions.Options;

/// <content>
/// Provides additional content specific to ASP.NET Core.
/// </content>
public partial class DeprecationPolicyManager
{
    private readonly IOptions<ApiVersioningOptions> options;

    /// <inheritdoc/>
    protected override ApiVersioningOptions Options => options.Value;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeprecationPolicyManager"/> class.
    /// </summary>
    /// <param name="options">The associated <see cref="ApiVersioningOptions">API versioning options</see>.</param>
    public DeprecationPolicyManager( IOptions<ApiVersioningOptions> options ) => this.options = options;
}