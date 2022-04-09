// Copyright (c) .NET Foundation and contributors. All rights reserved.

// NOTE: if the ASP.NET team fixes the design in .NET 7, then this entire file and class should go away
// REF: https://github.com/dotnet/aspnetcore/issues/39604
namespace Microsoft.AspNetCore.Http;

using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using static System.Linq.Expressions.Expression;

/// <summary>
/// Provides API Explorer extension methods for <see cref="IEndpointConventionBuilder"/>.
/// </summary>
[CLSCompliant( false )]
public static class ApiExplorerBuilderExtensions
{
    private static ExcludeFromDescriptionAttribute? excludeFromDescriptionMetadataAttribute;
    private static Func<Type, int, IProducesResponseTypeMetadata>? newProducesResponseTypeMetadata2;
    private static Func<Type, int, string, string[], IProducesResponseTypeMetadata>? newProducesResponseTypeMetadata4;
    private static Func<Type, bool, string[], IAcceptsMetadata>? newAcceptsMetadata3;

    /// <summary>
    /// Adds the <see cref="IExcludeFromDescriptionMetadata"/> to <see cref="EndpointBuilder.Metadata"/> for all endpoints.
    /// </summary>
    /// <typeparam name="TBuilder">The type of <see cref="IEndpointConventionBuilder"/>.</typeparam>
    /// <param name="builder">The extended <see cref="IEndpointConventionBuilder">endpoint convention builder</see>.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static TBuilder ExcludeFromDescription<TBuilder>( this TBuilder builder )
        where TBuilder : IEndpointConventionBuilder
    {
        excludeFromDescriptionMetadataAttribute ??= new();
        builder.WithMetadata( excludeFromDescriptionMetadataAttribute );
        return builder;
    }

    /// <summary>
    /// Adds an <see cref="IProducesResponseTypeMetadata"/> to <see cref="EndpointBuilder.Metadata"/> for all endpoints.
    /// </summary>
    /// <typeparam name="TBuilder">The type of <see cref="IEndpointConventionBuilder"/>.</typeparam>
    /// <param name="builder">The extended <see cref="IEndpointConventionBuilder">endpoint convention builder</see>.</param>
    /// <param name="build">The <see cref="Action{T}"/> used to build the response metadata.</param>
    /// <param name="statusCode">The response status code. Defaults to <see cref="StatusCodes.Status200OK"/>.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static TBuilder Produces<TBuilder>(
        this TBuilder builder,
        Action<ProducesResponseMetadataBuilder<TBuilder>> build,
        int statusCode = StatusCodes.Status200OK )
        where TBuilder : IEndpointConventionBuilder
    {
        if ( build == null )
        {
            throw new ArgumentNullException( nameof( build ) );
        }

        var metadata = new ProducesResponseMetadataBuilder<TBuilder>( builder, statusCode );
        build( metadata );
        metadata.Build();
        return builder;
    }

    /// <summary>
    /// Adds an <see cref="IProducesResponseTypeMetadata"/> to <see cref="EndpointBuilder.Metadata"/> for all endpoints.
    /// </summary>
    /// <typeparam name="TBuilder">The type of <see cref="IEndpointConventionBuilder"/>.</typeparam>
    /// <param name="builder">The extended <see cref="IEndpointConventionBuilder">endpoint convention builder</see>.</param>
    /// <param name="statusCode">The response status code.</param>
    /// <param name="responseType">The type of the response. Defaults to null.</param>
    /// <param name="contentType">The response content type. Defaults to "application/json" if responseType is not null, otherwise defaults to null.</param>
    /// <param name="additionalContentTypes">Additional response content types the endpoint produces for the supplied status code.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static TBuilder Produces<TBuilder>(
        this TBuilder builder,
        int statusCode,
        Type? responseType = null,
        string? contentType = null,
        params string[] additionalContentTypes )
        where TBuilder : IEndpointConventionBuilder
    {
        if ( responseType is not null && string.IsNullOrEmpty( contentType ) )
        {
            contentType = "application/json";
        }

        responseType ??= typeof( void );
        IProducesResponseTypeMetadata metadata;

        if ( contentType is null )
        {
            newProducesResponseTypeMetadata2 ??= NewProducesResponseTypeMetadataFunc2();
            metadata = newProducesResponseTypeMetadata2( responseType, statusCode );
        }
        else
        {
            newProducesResponseTypeMetadata4 ??= NewProducesResponseTypeMetadataFunc4();
            metadata = newProducesResponseTypeMetadata4( responseType, statusCode, contentType, additionalContentTypes );
        }

        builder.WithMetadata( metadata );
        return builder;
    }

    /// <summary>
    /// Adds an <see cref="IProducesResponseTypeMetadata"/> with a <see cref="ProblemDetails"/> type
    /// to <see cref="EndpointBuilder.Metadata"/> for all endpoints.
    /// </summary>
    /// <typeparam name="TBuilder">The type of <see cref="IEndpointConventionBuilder"/>.</typeparam>
    /// <param name="builder">The extended <see cref="IEndpointConventionBuilder">endpoint convention builder</see>.</param>
    /// <param name="statusCode">The response status code.</param>
    /// <param name="contentType">The response content type. Defaults to "application/problem+json".</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static TBuilder ProducesProblem<TBuilder>(
        this TBuilder builder,
        int statusCode,
        string? contentType = null )
        where TBuilder : IEndpointConventionBuilder
    {
        if ( string.IsNullOrEmpty( contentType ) )
        {
            contentType = ProblemDetailsDefaults.MediaType.Json;
        }

        return Produces( builder, statusCode, typeof( ProblemDetails ), contentType );
    }

    /// <summary>
    /// Adds an <see cref="IProducesResponseTypeMetadata"/> with a <see cref="HttpValidationProblemDetails"/> type
    /// to <see cref="EndpointBuilder.Metadata"/> for all endpoints.
    /// </summary>
    /// <typeparam name="TBuilder">The type of <see cref="IEndpointConventionBuilder"/>.</typeparam>
    /// <param name="builder">The extended <see cref="IEndpointConventionBuilder">endpoint convention builder</see>.</param>
    /// <param name="statusCode">The response status code. Defaults to <see cref="StatusCodes.Status400BadRequest"/>.</param>
    /// <param name="contentType">The response content type. Defaults to "application/problem+json".</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static TBuilder ProducesValidationProblem<TBuilder>(
        this TBuilder builder,
        int statusCode = StatusCodes.Status400BadRequest,
        string? contentType = null )
        where TBuilder : IEndpointConventionBuilder
    {
        if ( string.IsNullOrEmpty( contentType ) )
        {
            contentType = ProblemDetailsDefaults.MediaType.Json;
        }

        return Produces( builder, statusCode, typeof( HttpValidationProblemDetails ), contentType );
    }

    /// <summary>
    /// Adds the <see cref="ITagsMetadata"/> to <see cref="EndpointBuilder.Metadata"/> for all endpoints.
    /// </summary>
    /// <typeparam name="TBuilder">The type of <see cref="IEndpointConventionBuilder"/>.</typeparam>
    /// <param name="builder">The extended <see cref="IEndpointConventionBuilder">endpoint convention builder</see>.</param>
    /// <param name="tags">A collection of tags to be associated with the endpoint.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    /// <remarks>When used with OpenAPI, the specification supports a tags classification to categorize
    /// operations into related groups. These tags are typically included in the generated specification
    /// and are typically used to group operations by tags in the UI.</remarks>
    public static TBuilder WithTags<TBuilder>( this TBuilder builder, params string[] tags )
         where TBuilder : IEndpointConventionBuilder => builder.WithMetadata( new TagsAttribute( tags ) );

    /// <summary>
    /// Adds <see cref="IAcceptsMetadata"/> to <see cref="EndpointBuilder.Metadata"/> for all endpoints.
    /// </summary>
    /// <typeparam name="TBuilder">The type of <see cref="IEndpointConventionBuilder"/>.</typeparam>
    /// <param name="builder">The extended <see cref="IEndpointConventionBuilder">endpoint convention builder</see>.</param>
    /// <param name="build">The <see cref="Action{T}"/> used to build the request metadata.</param>
    /// <param name="isOptional">Sets a value that determines if the request body is optional.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static TBuilder Accepts<TBuilder>(
        this TBuilder builder,
        Action<AcceptsMetadataBuilder<TBuilder>> build,
        bool isOptional = false)
        where TBuilder : IEndpointConventionBuilder
    {
        if ( build == null )
        {
            throw new ArgumentNullException( nameof( build ) );
        }

        var metadata = new AcceptsMetadataBuilder<TBuilder>( builder, isOptional );
        build( metadata );
        metadata.Build();
        return builder;
    }

    /// <summary>
    /// Adds <see cref="IAcceptsMetadata"/> to <see cref="EndpointBuilder.Metadata"/> for all endpoints.
    /// </summary>
    /// <typeparam name="TBuilder">The type of <see cref="IEndpointConventionBuilder"/>.</typeparam>
    /// <param name="builder">The extended <see cref="IEndpointConventionBuilder">endpoint convention builder</see>.</param>
    /// <param name="requestType">The type of the request body.</param>
    /// <param name="contentType">The request content type that the endpoint accepts.</param>
    /// <param name="additionalContentTypes">The list of additional request content types that the endpoint accepts.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static TBuilder Accepts<TBuilder>(
        this TBuilder builder,
        Type requestType,
        string contentType,
        params string[] additionalContentTypes )
        where TBuilder : IEndpointConventionBuilder
    {
        newAcceptsMetadata3 ??= NewAcceptsMetadataFunc3();

        var allContentTypes = GetAllContentTypes( contentType, additionalContentTypes ?? Array.Empty<string>() );
        var metadata = newAcceptsMetadata3( requestType, false, allContentTypes );

        return builder.WithMetadata( metadata );
    }

    /// <summary>
    /// Adds <see cref="IAcceptsMetadata"/> to <see cref="EndpointBuilder.Metadata"/> for all endpoints
    /// produced by <paramref name="builder"/>.
    /// </summary>
    /// <typeparam name="TBuilder">The type of <see cref="IEndpointConventionBuilder"/>.</typeparam>
    /// <param name="builder">The extended <see cref="IEndpointConventionBuilder">endpoint convention builder</see>.</param>
    /// <param name="requestType">The type of the request body.</param>
    /// <param name="isOptional">Sets a value that determines if the request body is optional.</param>
    /// <param name="contentType">The request content type that the endpoint accepts.</param>
    /// <param name="additionalContentTypes">The list of additional request content types that the endpoint accepts.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static TBuilder Accepts<TBuilder>(
        this TBuilder builder,
        Type requestType,
        bool isOptional,
        string contentType,
        params string[] additionalContentTypes )
        where TBuilder : IEndpointConventionBuilder
    {
        newAcceptsMetadata3 ??= NewAcceptsMetadataFunc3();

        var allContentTypes = GetAllContentTypes( contentType, additionalContentTypes ?? Array.Empty<string>() );
        var metadata = newAcceptsMetadata3( requestType, isOptional, allContentTypes );

        return builder.WithMetadata( metadata );
    }

    private static string[] GetAllContentTypes( string contentType, string[] additionalContentTypes )
    {
        var allContentTypes = new string[additionalContentTypes.Length + 1];
        allContentTypes[0] = contentType;

        for ( var i = 0; i < additionalContentTypes.Length; i++ )
        {
            allContentTypes[i + 1] = additionalContentTypes[i];
        }

        return allContentTypes;
    }

    // HACK: >_< these are internal types and can't be forked due to internal logic and members
    // REF: https://github.com/dotnet/aspnetcore/blob/main/src/Shared/ApiExplorerTypes/ProducesResponseTypeMetadata.cs
    // REF: https://github.com/dotnet/aspnetcore/blob/main/src/Shared/RoutingMetadata/AcceptsMetadata.cs
    private static class TypeNames
    {
        private const string Assembly = "Microsoft.AspNetCore.Routing";
        private const string Namespace = "Microsoft.AspNetCore.Http";

        public const string ProducesResponseTypeMetadata = $"{Namespace}.{nameof( ProducesResponseTypeMetadata )}, {Assembly}";
        public const string AcceptsMetadata = $"{Namespace}.Metadata.{nameof( AcceptsMetadata )}, {Assembly}";
    }

    private static Func<Type, int, IProducesResponseTypeMetadata> NewProducesResponseTypeMetadataFunc2()
    {
        var @class = Type.GetType( TypeNames.ProducesResponseTypeMetadata, throwOnError: true, ignoreCase: false )!;
        var type = Parameter( typeof( Type ), "type" );
        var statusCode = Parameter( typeof( int ), "statusCode" );
        var ctor = @class.GetConstructor( new[] { typeof( Type ), typeof( int ) } )!;
        var body = New( ctor, type, statusCode );
        var lambda = Lambda<Func<Type, int, IProducesResponseTypeMetadata>>( body, type, statusCode );

        return lambda.Compile();
    }

    private static Func<Type, int, string, string[], IProducesResponseTypeMetadata> NewProducesResponseTypeMetadataFunc4()
    {
        var @class = Type.GetType( TypeNames.ProducesResponseTypeMetadata, throwOnError: true, ignoreCase: false )!;
        var type = Parameter( typeof( Type ), "type" );
        var statusCode = Parameter( typeof( int ), "statusCode" );
        var contentType = Parameter( typeof( string ), "contentType" );
        var additionalContentTypes = Parameter( typeof( string[] ), "additionalContentTypes" );
        var ctor = @class.GetConstructor( new[] { typeof( Type ), typeof( int ), typeof( string ), typeof( string[] ) } )!;
        var body = New( ctor, type, statusCode, contentType, additionalContentTypes );
        var lambda = Lambda<Func<Type, int, string, string[], IProducesResponseTypeMetadata>>( body, type, statusCode, contentType, additionalContentTypes );

        return lambda.Compile();
    }

    private static Func<Type, bool, string[], IAcceptsMetadata> NewAcceptsMetadataFunc3()
    {
        var @class = Type.GetType( TypeNames.AcceptsMetadata, throwOnError: true, ignoreCase: false )!;
        var type = Parameter( typeof( Type ), "type" );
        var isOptional = Parameter( typeof( bool ), "isOptional" );
        var contentTypes = Parameter( typeof( string[] ), "contentTypes" );
        var ctor = @class.GetConstructor( new[] { typeof( Type ), typeof( bool ), typeof( string[] ) } )!;
        var body = New( ctor, type, isOptional, contentTypes );
        var lambda = Lambda<Func<Type, bool, string[], IAcceptsMetadata>>( body, type, isOptional, contentTypes );

        return lambda.Compile();
    }
}