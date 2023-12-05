// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Microsoft.Extensions.DependencyInjection;

using Asp.Versioning;
using Asp.Versioning.Http;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
#if NETSTANDARD2_0
using DateOnly = System.DateTime;
#endif

/// <summary>
/// Provides extension methods for <see cref="IHttpClientBuilder"/>.
/// </summary>
public static class IHttpClientBuilderExtensions
{
    /// <summary>
    /// Adds the specified API version to the corresponding HTTP client.
    /// </summary>
    /// <param name="builder">The extended <see cref="IHttpClientBuilder">HTTP client builder</see>.</param>
    /// <param name="majorVersion">The major version number.</param>
    /// <param name="minorVersion">The optional minor version number.</param>
    /// <param name="status">The optional version status.</param>
    /// <param name="apiVersionWriter">The optional <see cref="IApiVersionWriter">API writer</see>.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    /// <remarks>If <paramref name="apiVersionWriter"/> is not provided, then an instance will be
    /// resolved from the associated <see cref="IServiceCollection"/>.</remarks>
    public static IHttpClientBuilder AddApiVersion(
        this IHttpClientBuilder builder,
        int majorVersion,
        int? minorVersion = default,
        string? status = default,
        IApiVersionWriter? apiVersionWriter = default ) =>
        builder.AddApiVersion( new ApiVersion( majorVersion, minorVersion, status ), apiVersionWriter );

    /// <summary>
    /// Adds the specified API version to the corresponding HTTP client.
    /// </summary>
    /// <param name="builder">The extended <see cref="IHttpClientBuilder">HTTP client builder</see>.</param>
    /// <param name="version">The version number.</param>
    /// <param name="status">The optional version status.</param>
    /// <param name="apiVersionWriter">The optional <see cref="IApiVersionWriter">API writer</see>.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    /// <remarks>If <paramref name="apiVersionWriter"/> is not provided, then an instance will be
    /// resolved from the associated <see cref="IServiceCollection"/>.</remarks>
    public static IHttpClientBuilder AddApiVersion(
        this IHttpClientBuilder builder,
        double version,
        string? status = default,
        IApiVersionWriter? apiVersionWriter = default ) =>
        builder.AddApiVersion( new ApiVersion( version, status ), apiVersionWriter );

    /// <summary>
    /// Adds the specified API version to the corresponding HTTP client.
    /// </summary>
    /// <param name="builder">The extended <see cref="IHttpClientBuilder">HTTP client builder</see>.</param>
    /// <param name="year">The version year.</param>
    /// <param name="month">The version month.</param>
    /// <param name="day">The version day.</param>
    /// <param name="status">The optional version status.</param>
    /// <param name="apiVersionWriter">The optional <see cref="IApiVersionWriter">API writer</see>.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    /// <remarks>If <paramref name="apiVersionWriter"/> is not provided, then an instance will be
    /// resolved from the associated <see cref="IServiceCollection"/>.</remarks>
    public static IHttpClientBuilder AddApiVersion(
        this IHttpClientBuilder builder,
        int year,
        int month,
        int day,
        string? status = default,
        IApiVersionWriter? apiVersionWriter = default ) =>
        builder.AddApiVersion( new ApiVersion( new DateOnly( year, month, day ), status ), apiVersionWriter );

    /// <summary>
    /// Adds the specified API version to the corresponding HTTP client.
    /// </summary>
    /// <param name="builder">The extended <see cref="IHttpClientBuilder">HTTP client builder</see>.</param>
    /// <param name="groupVersion">The group version.</param>
    /// <param name="status">The optional version status.</param>
    /// <param name="apiVersionWriter">The optional <see cref="IApiVersionWriter">API writer</see>.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    /// <remarks>If <paramref name="apiVersionWriter"/> is not provided, then an instance will be
    /// resolved from the associated <see cref="IServiceCollection"/>.</remarks>
    public static IHttpClientBuilder AddApiVersion(
        this IHttpClientBuilder builder,
        DateOnly groupVersion,
        string? status = default,
        IApiVersionWriter? apiVersionWriter = default ) =>
        builder.AddApiVersion( new ApiVersion( groupVersion, status ), apiVersionWriter );

    /// <summary>
    /// Adds the specified API version to the corresponding HTTP client.
    /// </summary>
    /// <param name="builder">The extended <see cref="IHttpClientBuilder">HTTP client builder</see>.</param>
    /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> added to requests.</param>
    /// <param name="apiVersionWriter">The optional <see cref="IApiVersionWriter">API writer</see>.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    /// <remarks>If <paramref name="apiVersionWriter"/> is not provided, then an instance will be
    /// resolved from the associated <see cref="IServiceCollection"/>.</remarks>
    public static IHttpClientBuilder AddApiVersion(
        this IHttpClientBuilder builder,
        ApiVersion apiVersion,
        IApiVersionWriter? apiVersionWriter = default )
    {
        ArgumentNullException.ThrowIfNull( builder );

        var services = builder.Services;

        services.TryAddSingleton<IApiVersionWriter, QueryStringApiVersionWriter>();
        services.TryAddSingleton<IApiVersionParser, ApiVersionParser>();
        services.TryAddTransient<ApiVersionHeaderEnumerable>();
        builder.AddHttpMessageHandler( sp => NewApiVersionHandler( sp, apiVersion, apiVersionWriter ) );

        return builder;
    }

    private static ApiVersionHandler NewApiVersionHandler(
        IServiceProvider serviceProvider,
        ApiVersion apiVersion,
        IApiVersionWriter? writer )
    {
        writer ??= serviceProvider.GetRequiredService<IApiVersionWriter>();
        var parser = serviceProvider.GetService<IApiVersionParser>();
        var notification = serviceProvider.GetService<IApiNotification>() ??
                           BuildFallbackNotification( serviceProvider, parser );

        return new( writer, apiVersion, notification, parser );
    }

    private static ApiVersionHandlerLogger<ApiVersionHandler>? BuildFallbackNotification(
        IServiceProvider serviceProvider,
        IApiVersionParser? parser )
    {
        var logger = serviceProvider.GetService<ILogger<ApiVersionHandler>>();

        if ( logger == null )
        {
            // AddLogging wasn't called
            return default;
        }

        var enumerable = serviceProvider.GetService<ApiVersionHeaderEnumerable>();

        return new( logger, parser ?? ApiVersionParser.Default, enumerable ?? new() );
    }
}