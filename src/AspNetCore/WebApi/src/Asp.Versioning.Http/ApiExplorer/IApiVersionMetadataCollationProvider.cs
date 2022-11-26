// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApiExplorer;

/// <summary>
/// Defines the behavior of an API version metadata collation provider.
/// </summary>
public interface IApiVersionMetadataCollationProvider
{
    /// <summary>
    /// Gets version of the underlying provider results.
    /// </summary>
    /// <value>The version of the provider results. This can be used to detect changes.</value>
    int Version { get; }

    /// <summary>
    /// Executes the provider using the given context.
    /// </summary>
    /// <param name="context">The collation context.</param>
    void Execute( ApiVersionMetadataCollationContext context );
}