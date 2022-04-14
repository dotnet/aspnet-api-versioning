// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Builder;

/// <summary>
/// Provides extension methods for endpoint metadata builders.
/// </summary>
[CLSCompliant( false )]
public static class IEndpointMetadataBuilderExtensions
{
    /// <summary>
    /// Configures the API to report compatible versions.
    /// </summary>
    /// <typeparam name="T">The extended type of <see cref="IVersionedApiBuilder"/>.</typeparam>
    /// <param name="builder">The extended Minimal API convention builder.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static T ReportApiVersions<T>( this T builder )
        where T : notnull, IEndpointMetadataBuilder
    {
        builder.ReportApiVersions = true;
        return builder;
    }
}