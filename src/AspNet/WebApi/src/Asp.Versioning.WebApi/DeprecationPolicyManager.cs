// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

/// <content>
/// Provides additional content specific to ASP.NET Web API.
/// </content>
public partial class DeprecationPolicyManager
{
    private readonly ApiVersioningOptions options;

    /// <inheritdoc/>
    protected override ApiVersioningOptions Options => options;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeprecationPolicyManager"/> class.
    /// </summary>
    /// <param name="options">The associated <see cref="ApiVersioningOptions">API versioning options</see>.</param>
    public DeprecationPolicyManager( ApiVersioningOptions options ) => this.options = options;
}