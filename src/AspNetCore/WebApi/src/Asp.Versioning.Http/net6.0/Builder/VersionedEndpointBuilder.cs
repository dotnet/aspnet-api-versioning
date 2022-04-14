// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Builder;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;

/// <summary>
/// Represents a Minimal API endpoint builder.
/// </summary>
[CLSCompliant( false )]
public class VersionedEndpointBuilder : IVersionedEndpointBuilder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VersionedEndpointBuilder"/> class.
    /// </summary>
    /// <param name="builder">The underlying Minimal API builder.</param>
    public VersionedEndpointBuilder( IVersionedApiBuilder builder ) => ApiBuilder = builder;

    /// <summary>
    /// Gets the underlying Minimal API builder.
    /// </summary>
    /// <value>The underlying <see cref="IVersionedApiBuilder">Minimal API builder</see>.</value>
    protected IVersionedApiBuilder ApiBuilder { get; }

    private VersionedEndpointDataSource DataSource
    {
        get
        {
            var sources = ApiBuilder.DataSources;

            if ( sources.OfType<VersionedEndpointDataSource>().FirstOrDefault() is not VersionedEndpointDataSource source )
            {
                sources.Add( source = new() );
            }

            return source;
        }
    }

    /// <summary>
    /// Adds an endpoint to the collection that matches HTTP requests for the specified pattern.
    /// </summary>
    /// <param name="pattern">The route pattern.</param>
    /// <param name="requestDelegate">The delegate executed when the endpoint is matched.</param>
    /// <returns>A <see cref="EndpointMetadataBuilder"/> that can be used to further customize the endpoint.</returns>
    public virtual EndpointMetadataBuilder Map( RoutePattern pattern, RequestDelegate requestDelegate )
    {
        if ( requestDelegate == null )
        {
            throw new ArgumentNullException( nameof( requestDelegate ) );
        }

        if ( pattern == null )
        {
            throw new ArgumentNullException( nameof( pattern ) );
        }

        var endpointBuilder = new RouteEndpointBuilder( requestDelegate, pattern, order: default )
        {
            DisplayName = ApiBuilder.Name ?? pattern.RawText,
        };
        var conventionBuilder = DataSource.Add( endpointBuilder );
        var metadataBuilder = new EndpointMetadataBuilder( ApiBuilder, conventionBuilder );

        conventionBuilder.Add( new VersionedEndpointMetadataConvention( metadataBuilder ) );

        if ( requestDelegate.Method.GetCustomAttributes() is IEnumerable<Attribute> attributes )
        {
            foreach ( var attribute in attributes )
            {
                endpointBuilder.Metadata.Add( attribute );
            }
        }

        return metadataBuilder;
    }

    /// <summary>
    /// Adds an endpoint to the collection that matches HTTP requests for the specified HTTP methods and pattern.
    /// </summary>
    /// <param name="pattern">The route pattern.</param>
    /// <param name="httpMethods">HTTP methods that the endpoint will match.</param>
    /// <param name="handler">The delegate executed when the endpoint is matched.</param>
    /// <returns>A <see cref="EndpointMetadataBuilder"/> that can be used to further customize the endpoint.</returns>
    public virtual EndpointMetadataBuilder MapMethods( string pattern, IEnumerable<string> httpMethods, Delegate handler )
    {
        if ( httpMethods is null )
        {
            throw new ArgumentNullException( nameof( httpMethods ) );
        }

        if ( handler is null )
        {
            throw new ArgumentNullException( nameof( handler ) );
        }

        var routePattern = RoutePatternFactory.Parse( pattern );
        var routeParams = new List<string>( routePattern.Parameters.Count );

        for ( var i = 0; i < routePattern.Parameters.Count; i++ )
        {
            routeParams.Add( routePattern.Parameters[i].Name );
        }

        var services = ApiBuilder.ServiceProvider;
        var routeHandlerOptions = services.GetService<IOptions<RouteHandlerOptions>>();
        var disableInferBodyFromParameters = httpMethods.Any( ShouldDisableInferredBody );
        var options = new RequestDelegateFactoryOptions()
        {
            ServiceProvider = services,
            RouteParameterNames = routeParams,
            ThrowOnBadRequest = routeHandlerOptions?.Value.ThrowOnBadRequest ?? false,
            DisableInferBodyFromParameters = disableInferBodyFromParameters,
        };
        var requestDelegateResult = RequestDelegateFactory.Create( handler, options );
        var endpointBuilder = new RouteEndpointBuilder( requestDelegateResult.RequestDelegate, routePattern, order: default )
        {
            DisplayName = ApiBuilder.Name ?? routePattern.RawText ?? pattern,
        };
        var conventionBuilder = DataSource.Add( endpointBuilder );
        var metadataBuilder = new EndpointMetadataBuilder( ApiBuilder, conventionBuilder );

        conventionBuilder.Add( new VersionedEndpointMetadataConvention( metadataBuilder ) );
        endpointBuilder.Metadata.Add( handler.Method );
        endpointBuilder.Metadata.Add( new HttpMethodMetadata( httpMethods ) );

        for ( var i = 0; i < requestDelegateResult.EndpointMetadata.Count; i++ )
        {
            endpointBuilder.Metadata.Add( requestDelegateResult.EndpointMetadata[i] );
        }

        if ( handler.Method.GetCustomAttributes() is IEnumerable<Attribute> attributes )
        {
            foreach ( var attribute in attributes )
            {
                endpointBuilder.Metadata.Add( attribute );
            }
        }

        return metadataBuilder;
    }

    /// <summary>
    /// Determines whether inferring the HTTP body should be disabled.
    /// </summary>
    /// <param name="method">The HTTP method to evaluate.</param>
    /// <returns>True if the HTTP body should not be inferred; otherwise, false.</returns>
    protected static bool ShouldDisableInferredBody( string method )
    {
        if ( string.IsNullOrEmpty( method ) )
        {
            throw new ArgumentNullException( nameof( method ) );
        }

        // GET, DELETE, HEAD, CONNECT, TRACE, and OPTIONS normally do not contain bodies
        return method.Equals( HttpMethods.Get, StringComparison.Ordinal ) ||
               method.Equals( HttpMethods.Delete, StringComparison.Ordinal ) ||
               method.Equals( HttpMethods.Head, StringComparison.Ordinal ) ||
               method.Equals( HttpMethods.Options, StringComparison.Ordinal ) ||
               method.Equals( HttpMethods.Trace, StringComparison.Ordinal ) ||
               method.Equals( HttpMethods.Connect, StringComparison.Ordinal );
    }

    /// <summary>
    /// Gets the endpoint name for the specified method.
    /// </summary>
    /// <param name="method">The method to get the name for.</param>
    /// <returns>The endpoint name.</returns>
    protected static string GetEndpointName( MethodInfo method )
    {
        if ( method == null )
        {
            throw new ArgumentNullException( nameof( method ) );
        }

        if ( TryParseLocalFunctionName( method.Name, out var endpointName ) )
        {
            return endpointName;
        }

        return method.Name;
    }

    private static bool TryParseLocalFunctionName( string generatedName, [NotNullWhen( true )] out string? originalName )
    {
        var startIndex = generatedName.LastIndexOf( ">g__", StringComparison.Ordinal );
        var endIndex = generatedName.LastIndexOf( "|", StringComparison.Ordinal );

        if ( startIndex >= 0 && endIndex >= 0 && endIndex - startIndex > 4 )
        {
            originalName = generatedName.Substring( startIndex + 4, endIndex - startIndex - 4 );
            return true;
        }

        originalName = null;
        return false;
    }

    /// <summary>
    /// Determines whether the specified method is compiler-generated.
    /// </summary>
    /// <param name="method">The method to evaluate.</param>
    /// <returns>True if the <paramref name="method"/> is compiler-generated; otherwise, false.</returns>
    protected static bool IsCompilerGeneratedMethod( MethodInfo method )
    {
        if ( method == null )
        {
            throw new ArgumentNullException( nameof( method ) );
        }

        return Attribute.IsDefined( method, typeof( CompilerGeneratedAttribute ) ) || IsCompilerGeneratedType( method.DeclaringType );
    }

    private static bool IsCompilerGeneratedType( Type? type = null ) =>
        type is not null && ( Attribute.IsDefined( type, typeof( CompilerGeneratedAttribute ) ) || IsCompilerGeneratedType( type.DeclaringType ) );

    IEndpointMetadataBuilder IVersionedEndpointBuilder.Map( RoutePattern pattern, RequestDelegate requestDelegate ) => Map( pattern, requestDelegate );

    IEndpointMetadataBuilder IVersionedEndpointBuilder.MapMethods( string pattern, IEnumerable<string> httpMethods, Delegate handler ) => MapMethods( pattern, httpMethods, handler );
}