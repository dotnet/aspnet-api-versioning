// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

#if NETSTANDARD
using DateOnly = System.DateTime;
#endif

/// <summary>
/// Provides extension methods for the <see cref="IApiVersioningPolicyBuilder"/> interface.
/// </summary>
public static class IApiVersioningPolicyBuilderExtensions
{
    /// <summary>
    /// Creates and returns a new sunset policy builder.
    /// </summary>
    /// <param name="builder">The extended <see cref="IApiVersioningPolicyBuilder">API versioning policy builder</see>.</param>
    /// <param name="name">The name of the API the policy is for.</param>
    /// <returns>A new <see cref="ISunsetPolicyBuilder">sunset policy builder</see>.</returns>
    public static ISunsetPolicyBuilder Sunset( this IApiVersioningPolicyBuilder builder, string name )
    {
        ArgumentNullException.ThrowIfNull( builder );
        return builder.Sunset( name, default );
    }

    /// <summary>
    /// Creates and returns a new sunset policy builder.
    /// </summary>
    /// <param name="builder">The extended <see cref="IApiVersioningPolicyBuilder">API versioning policy builder</see>.</param>
    /// <param name="name">The name of the API the policy is for.</param>
    /// <param name="majorVersion">The major version number.</param>
    /// <param name="minorVersion">The optional minor version number.</param>
    /// <param name="status">The optional version status.</param>
    /// <returns>A new <see cref="ISunsetPolicyBuilder">sunset policy builder</see>.</returns>
    public static ISunsetPolicyBuilder Sunset(
        this IApiVersioningPolicyBuilder builder,
        string name,
        int majorVersion,
        int? minorVersion = default,
        string? status = default )
    {
        ArgumentNullException.ThrowIfNull( builder );
        return builder.Sunset( name, new ApiVersion( majorVersion, minorVersion, status ) );
    }

    /// <summary>
    /// Creates and returns a new sunset policy builder.
    /// </summary>
    /// <param name="builder">The extended <see cref="IApiVersioningPolicyBuilder">API versioning policy builder</see>.</param>
    /// <param name="name">The name of the API the policy is for.</param>
    /// <param name="version">The version number.</param>
    /// <param name="status">The optional version status.</param>
    /// <returns>A new <see cref="ISunsetPolicyBuilder">sunset policy builder</see>.</returns>
    public static ISunsetPolicyBuilder Sunset( this IApiVersioningPolicyBuilder builder, string name, double version, string? status = default )
    {
        ArgumentNullException.ThrowIfNull( builder );
        return builder.Sunset( name, new ApiVersion( version, status ) );
    }

    /// <summary>
    /// Creates and returns a new sunset policy builder.
    /// </summary>
    /// <param name="builder">The extended <see cref="IApiVersioningPolicyBuilder">API versioning policy builder</see>.</param>
    /// <param name="name">The name of the API the policy is for.</param>
    /// <param name="year">The version year.</param>
    /// <param name="month">The version month.</param>
    /// <param name="day">The version day.</param>
    /// <param name="status">The optional version status.</param>
    /// <returns>A new <see cref="ISunsetPolicyBuilder">sunset policy builder</see>.</returns>
    public static ISunsetPolicyBuilder Sunset( this IApiVersioningPolicyBuilder builder, string name, int year, int month, int day, string? status = default )
    {
        ArgumentNullException.ThrowIfNull( builder );
        return builder.Sunset( name, new ApiVersion( new DateOnly( year, month, day ), status ) );
    }

    /// <summary>
    /// Creates and returns a new sunset policy builder.
    /// </summary>
    /// <param name="builder">The extended <see cref="IApiVersioningPolicyBuilder">API versioning policy builder</see>.</param>
    /// <param name="name">The name of the API the policy is for.</param>
    /// <param name="groupVersion">The group version.</param>
    /// <param name="status">The optional version status.</param>
    /// <returns>A new <see cref="ISunsetPolicyBuilder">sunset policy builder</see>.</returns>
    public static ISunsetPolicyBuilder Sunset( this IApiVersioningPolicyBuilder builder, string name, DateOnly groupVersion, string? status = default )
    {
        ArgumentNullException.ThrowIfNull( builder );
        return builder.Sunset( name, new ApiVersion( groupVersion, status ) );
    }

    /// <summary>
    /// Creates and returns a new sunset policy builder.
    /// </summary>
    /// <param name="builder">The extended <see cref="IApiVersioningPolicyBuilder">API versioning policy builder</see>.</param>
    /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> the policy is for.</param>
    /// <returns>A new <see cref="ISunsetPolicyBuilder">sunset policy builder</see>.</returns>
    public static ISunsetPolicyBuilder Sunset( this IApiVersioningPolicyBuilder builder, ApiVersion apiVersion )
    {
        ArgumentNullException.ThrowIfNull( builder );
        return builder.Sunset( default, apiVersion );
    }

    /// <summary>
    /// Creates and returns a new sunset policy builder.
    /// </summary>
    /// <param name="builder">The extended <see cref="IApiVersioningPolicyBuilder">API versioning policy builder</see>.</param>
    /// <param name="majorVersion">The major version number.</param>
    /// <param name="minorVersion">The optional minor version number.</param>
    /// <param name="status">The optional version status.</param>
    /// <returns>A new <see cref="ISunsetPolicyBuilder">sunset policy builder</see>.</returns>
    public static ISunsetPolicyBuilder Sunset(
        this IApiVersioningPolicyBuilder builder,
        int majorVersion,
        int? minorVersion = default,
        string? status = default )
    {
        ArgumentNullException.ThrowIfNull( builder );
        return builder.Sunset( default, new ApiVersion( majorVersion, minorVersion, status ) );
    }

    /// <summary>
    /// Creates and returns a new sunset policy builder.
    /// </summary>
    /// <param name="builder">The extended <see cref="IApiVersioningPolicyBuilder">API versioning policy builder</see>.</param>
    /// <param name="version">The version number.</param>
    /// <param name="status">The optional version status.</param>
    /// <returns>A new <see cref="ISunsetPolicyBuilder">sunset policy builder</see>.</returns>
    public static ISunsetPolicyBuilder Sunset( this IApiVersioningPolicyBuilder builder, double version, string? status = default )
    {
        ArgumentNullException.ThrowIfNull( builder );
        return builder.Sunset( default, new ApiVersion( version, status ) );
    }

    /// <summary>
    /// Creates and returns a new sunset policy builder.
    /// </summary>
    /// <param name="builder">The extended <see cref="IApiVersioningPolicyBuilder">API versioning policy builder</see>.</param>
    /// <param name="year">The version year.</param>
    /// <param name="month">The version month.</param>
    /// <param name="day">The version day.</param>
    /// <param name="status">The optional version status.</param>
    /// <returns>A new <see cref="ISunsetPolicyBuilder">sunset policy builder</see>.</returns>
    public static ISunsetPolicyBuilder Sunset( this IApiVersioningPolicyBuilder builder, int year, int month, int day, string? status = default )
    {
        ArgumentNullException.ThrowIfNull( builder );
        return builder.Sunset( default, new ApiVersion( new DateOnly( year, month, day ), status ) );
    }

    /// <summary>
    /// Creates and returns a new sunset policy builder.
    /// </summary>
    /// <param name="builder">The extended <see cref="IApiVersioningPolicyBuilder">API versioning policy builder</see>.</param>
    /// <param name="groupVersion">The group version.</param>
    /// <param name="status">The optional version status.</param>
    /// <returns>A new <see cref="ISunsetPolicyBuilder">sunset policy builder</see>.</returns>
    public static ISunsetPolicyBuilder Sunset( this IApiVersioningPolicyBuilder builder, DateOnly groupVersion, string? status = default )
    {
        ArgumentNullException.ThrowIfNull( builder );
        return builder.Sunset( default, new ApiVersion( groupVersion, status ) );
    }
}