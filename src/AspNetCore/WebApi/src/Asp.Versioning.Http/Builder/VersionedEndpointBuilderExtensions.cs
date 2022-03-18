// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Microsoft.AspNetCore.Builder;

using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

/// <summary>
/// Provides extension methods for endpoint route builders.
/// </summary>
[CLSCompliant( false )]
public static class VersionedEndpointBuilderExtensions
{
    private static readonly string[] GetVerb = new[] { HttpMethods.Get };
    private static readonly string[] PostVerb = new[] { HttpMethods.Post };
    private static readonly string[] PutVerb = new[] { HttpMethods.Put };
    private static readonly string[] DeleteVerb = new[] { HttpMethods.Delete };
    private static readonly string[] PatchVerb = new[] { HttpMethods.Patch };

    /// <summary>
    /// Defines a new API using the specified endpoint route builder.
    /// </summary>
    /// <param name="endpoints">The extend endpoint route builder.</param>
    /// <param name="name">The optional name of the API.</param>
    /// <returns>A new <see cref="VersionedApiBuilder">Minimal API convention builder</see>.</returns>
    public static VersionedApiBuilder DefineApi( this IEndpointRouteBuilder endpoints, string? name = default ) => new( endpoints, name );

    /// <summary>
    /// Configures the API to report compatible versions.
    /// </summary>
    /// <typeparam name="T">The extended type of <see cref="IVersionedApiBuilder"/>.</typeparam>
    /// <param name="builder">The extended Minimal API convention builder.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static T ReportApiVersions<T>( this T builder )
        where T : notnull, IVersionedApiBuilder
    {
        builder.ReportApiVersions = true;
        return builder;
    }

    /// <summary>
    /// Adds an endpoint to the builder that matches HTTP GET requests for the specified pattern.
    /// </summary>
    /// <param name="builder">the builder to add the route to.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <param name="requestDelegate">The delegate executed when the endpoint is matched.</param>
    /// <returns>A <see cref="EndpointMetadataBuilder"/> that can be used to further customize the endpoint.</returns>
    public static EndpointMetadataBuilder MapGet( this VersionedEndpointBuilder builder, string pattern, RequestDelegate requestDelegate )
    {
        if ( builder == null )
        {
            throw new ArgumentNullException( nameof( builder ) );
        }

        return builder.MapMethods( pattern, GetVerb, requestDelegate );
    }

    /// <summary>
    /// Adds an endpoint to the builder that matches HTTP GET requests for the specified pattern.
    /// </summary>
    /// <param name="builder">the builder to add the route to.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <param name="handler">The delegate executed when the endpoint is matched.</param>
    /// <returns>A <see cref="EndpointMetadataBuilder"/> that can be used to further customize the endpoint.</returns>
    public static EndpointMetadataBuilder MapGet( this VersionedEndpointBuilder builder, string pattern, Delegate handler )
    {
        if ( builder == null )
        {
            throw new ArgumentNullException( nameof( builder ) );
        }

        return builder.MapMethods( pattern, GetVerb, handler );
    }

    /// <summary>
    /// Adds an endpoint to the builder that matches HTTP POST requests for the specified pattern.
    /// </summary>
    /// <param name="builder">the builder to add the route to.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <param name="requestDelegate">The delegate executed when the endpoint is matched.</param>
    /// <returns>A <see cref="EndpointMetadataBuilder"/> that can be used to further customize the endpoint.</returns>
    public static EndpointMetadataBuilder MapPost( this VersionedEndpointBuilder builder, string pattern, RequestDelegate requestDelegate )
    {
        if ( builder == null )
        {
            throw new ArgumentNullException( nameof( builder ) );
        }

        return builder.MapMethods( pattern, PostVerb, requestDelegate );
    }

    /// <summary>
    /// Adds an endpoint to the builder that matches HTTP POST requests for the specified pattern.
    /// </summary>
    /// <param name="builder">the builder to add the route to.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <param name="handler">The delegate executed when the endpoint is matched.</param>
    /// <returns>A <see cref="EndpointMetadataBuilder"/> that can be used to further customize the endpoint.</returns>
    public static EndpointMetadataBuilder MapPost( this VersionedEndpointBuilder builder, string pattern, Delegate handler )
    {
        if ( builder == null )
        {
            throw new ArgumentNullException( nameof( builder ) );
        }

        return builder.MapMethods( pattern, PostVerb, handler );
    }

    /// <summary>
    /// Adds an endpoint to the builder that matches HTTP PUT requests
    /// for the specified pattern.
    /// </summary>
    /// <param name="builder">the builder to add the route to.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <param name="requestDelegate">The delegate executed when the endpoint is matched.</param>
    /// <returns>A <see cref="EndpointMetadataBuilder"/> that can be used to further customize the endpoint.</returns>
    public static EndpointMetadataBuilder MapPut( this VersionedEndpointBuilder builder, string pattern, RequestDelegate requestDelegate )
    {
        if ( builder == null )
        {
            throw new ArgumentNullException( nameof( builder ) );
        }

        return builder.MapMethods( pattern, PutVerb, requestDelegate );
    }

    /// <summary>
    /// Adds an endpoint to the builder that matches HTTP PUT requests
    /// for the specified pattern.
    /// </summary>
    /// <param name="builder">the builder to add the route to.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <param name="handler">The delegate executed when the endpoint is matched.</param>
    /// <returns>A <see cref="EndpointMetadataBuilder"/> that can be used to further customize the endpoint.</returns>
    public static EndpointMetadataBuilder MapPut( this VersionedEndpointBuilder builder, string pattern, Delegate handler )
    {
        if ( builder == null )
        {
            throw new ArgumentNullException( nameof( builder ) );
        }

        return builder.MapMethods( pattern, PutVerb, handler );
    }

    /// <summary>
    /// Adds an endpoint to the builder that matches HTTP DELETE requests
    /// for the specified pattern.
    /// </summary>
    /// <param name="builder">the builder to add the route to.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <param name="requestDelegate">The delegate executed when the endpoint is matched.</param>
    /// <returns>A <see cref="EndpointMetadataBuilder"/> that can be used to further customize the endpoint.</returns>
    public static EndpointMetadataBuilder MapDelete( this VersionedEndpointBuilder builder, string pattern, RequestDelegate requestDelegate )
    {
        if ( builder == null )
        {
            throw new ArgumentNullException( nameof( builder ) );
        }

        return builder.MapMethods( pattern, DeleteVerb, requestDelegate );
    }

    /// <summary>
    /// Adds an endpoint to the builder that matches HTTP DELETE requests
    /// for the specified pattern.
    /// </summary>
    /// <param name="builder">the builder to add the route to.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <param name="handler">The delegate executed when the endpoint is matched.</param>
    /// <returns>A <see cref="EndpointMetadataBuilder"/> that can be used to further customize the endpoint.</returns>
    public static EndpointMetadataBuilder MapDelete( this VersionedEndpointBuilder builder, string pattern, Delegate handler )
    {
        if ( builder == null )
        {
            throw new ArgumentNullException( nameof( builder ) );
        }

        return builder.MapMethods( pattern, DeleteVerb, handler );
    }

    /// <summary>
    /// Adds an endpoint to the builder that matches HTTP PATCH requests
    /// for the specified pattern.
    /// </summary>
    /// <param name="builder">the builder to add the route to.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <param name="requestDelegate">The delegate executed when the endpoint is matched.</param>
    /// <returns>A <see cref="EndpointMetadataBuilder"/> that can be used to further customize the endpoint.</returns>
    public static EndpointMetadataBuilder MapPatch( this VersionedEndpointBuilder builder, string pattern, RequestDelegate requestDelegate )
    {
        if ( builder == null )
        {
            throw new ArgumentNullException( nameof( builder ) );
        }

        return builder.MapMethods( pattern, PatchVerb, requestDelegate );
    }

    /// <summary>
    /// Adds an endpoint to the builder that matches HTTP PATCH requests
    /// for the specified pattern.
    /// </summary>
    /// <param name="builder">the builder to add the route to.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <param name="handler">The delegate executed when the endpoint is matched.</param>
    /// <returns>A <see cref="EndpointMetadataBuilder"/> that can be used to further customize the endpoint.</returns>
    public static EndpointMetadataBuilder MapPatch( this VersionedEndpointBuilder builder, string pattern, Delegate handler )
    {
        if ( builder == null )
        {
            throw new ArgumentNullException( nameof( builder ) );
        }

        return builder.MapMethods( pattern, PatchVerb, handler );
    }
}