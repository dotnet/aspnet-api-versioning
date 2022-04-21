// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Microsoft.AspNetCore.Builder;

using Asp.Versioning;
using Asp.Versioning.Builder;
using Asp.Versioning.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Globalization;
using static Asp.Versioning.ApiVersionParameterLocation;

/// <summary>
/// Provides extension methods for <see cref="IEndpointConventionBuilder"/> and <see cref="IVersionedEndpointConventionBuilder"/>.
/// </summary>
[CLSCompliant( false )]
public static class IEndpointConventionBuilderExtensions
{
    /// <summary>
    /// Indicates that the endpoint will use API versioning.
    /// </summary>
    /// <param name="builder">The extended <see cref="IEndpointConventionBuilder">builder</see>.</param>
    /// <param name="apiVersionSet">The <see cref="ApiVersionSet">API version set</see> the endpoint will use.</param>
    /// <returns>A new <see cref="IVersionedEndpointConventionBuilder"/> instance.</returns>
    /// <remarks>If the specified <paramref name="builder"/> already implements <see cref="IVersionedEndpointConventionBuilder"/>,
    /// then that instance will be returned instead.</remarks>
    public static IVersionedEndpointConventionBuilder UseApiVersioning(
        this IEndpointConventionBuilder builder,
        ApiVersionSet apiVersionSet )
    {
        if ( builder == null )
        {
            throw new ArgumentNullException( nameof( builder ) );
        }

        if ( apiVersionSet == null )
        {
            throw new ArgumentNullException( nameof( apiVersionSet ) );
        }

        if ( builder is IVersionedEndpointConventionBuilder versionedBuilder )
        {
            return versionedBuilder;
        }

        versionedBuilder = new VersionedEndpointConventionBuilder( builder, apiVersionSet );
        builder.Add( endpoints => Apply( endpoints, versionedBuilder, apiVersionSet ) );

        return versionedBuilder;
    }

    /// <summary>
    /// Indicates that the endpoint will report its API versions.
    /// </summary>
    /// <typeparam name="TBuilder">The type of <see cref="IVersionedEndpointConventionBuilder"/>.</typeparam>
    /// <param name="builder">The extended builder.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static TBuilder ReportApiVersions<TBuilder>( this TBuilder builder )
        where TBuilder : notnull, IVersionedEndpointConventionBuilder
    {
        if ( builder == null )
        {
            throw new ArgumentNullException( nameof( builder ) );
        }

        builder.ReportApiVersions = true;
        return builder;
    }

    private static void Apply(
        EndpointBuilder endpointBuilder,
        IVersionedEndpointConventionBuilder conventions,
        ApiVersionSet versionSet )
    {
        // this will change to EndpointBuilder.ServiceProvider in 7.0
        // REF: https://github.com/dotnet/aspnetcore/pull/41238/files#diff-f8807c470bcc3a077fb176668a46df57b4bb99c992b6b7b375665f8bf3903c94R510
        if ( versionSet.ServiceProvider is not IServiceProvider services )
        {
            throw new InvalidOperationException( SR.NoEndpointBuilderServices );
        }

        var parameterSource = services.GetRequiredService<IApiVersionParameterSource>();
        var options = services.GetRequiredService<IOptions<ApiVersioningOptions>>().Value;
        var requestDelegate = default( RequestDelegate );
        var metadata = conventions.Build( options );

        endpointBuilder.Metadata.Add( metadata );

        if ( options.ReportApiVersions ||
             versionSet.ReportApiVersions ||
             conventions.ReportApiVersions )
        {
            requestDelegate = EnsureRequestDelegate( requestDelegate, endpointBuilder.RequestDelegate );
            requestDelegate = new ReportApiVersionsDecorator( requestDelegate, metadata );
            endpointBuilder.RequestDelegate = requestDelegate;
        }

        if ( parameterSource.VersionsByMediaType() )
        {
            var parameterName = parameterSource.GetParameterName( MediaTypeParameter );
            requestDelegate = EnsureRequestDelegate( requestDelegate, endpointBuilder.RequestDelegate );
            requestDelegate = new ContentTypeApiVersionDecorator( requestDelegate, parameterName );
            endpointBuilder.RequestDelegate = requestDelegate;
        }
    }

    private static RequestDelegate EnsureRequestDelegate( RequestDelegate? current, RequestDelegate? original ) =>
        ( current ?? original ) ??
        throw new InvalidOperationException(
            string.Format(
                CultureInfo.CurrentCulture,
                SR.UnsetRequestDelegate,
                nameof( RequestDelegate ),
                nameof( RouteEndpoint ) ) );
}