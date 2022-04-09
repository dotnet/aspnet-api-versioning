// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Builder;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;

/// <summary>
/// Represents a builder for <see cref="IProducesResponseTypeMetadata"/>.
/// </summary>
/// <typeparam name="TBuilder">The type of <see cref="IEndpointConventionBuilder"/>.</typeparam>
[CLSCompliant( false )]
public class ProducesResponseMetadataBuilder<TBuilder>
    where TBuilder : IEndpointConventionBuilder
{
    private Type? responseType;
    private string? contentType;
    private string[]? additionalContentTypes;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProducesResponseMetadataBuilder{TBuilder}"/> class.
    /// </summary>
    /// <param name="builder">The associated endpoint builder.</param>
    /// <param name="statusCode">The configured status code.</param>
    public ProducesResponseMetadataBuilder( TBuilder builder, int statusCode )
    {
        Builder = builder;
        StatusCode = statusCode;
    }

    /// <summary>
    /// Gets the associated endpoint builder.
    /// </summary>
    /// <value>The associated endpoint builder.</value>
    protected TBuilder Builder { get; }

    /// <summary>
    /// Gets the configured status code.
    /// </summary>
    /// <value>The configured status code.</value>
    protected int StatusCode { get; }

    /// <summary>
    /// Adds the type of response that will be returned.
    /// </summary>
    /// <typeparam name="TBody">The type of response body.</typeparam>
    /// <returns>The original instance.</returns>
    public virtual ProducesResponseMetadataBuilder<TBuilder> Body<TBody>()
    {
        responseType = typeof( TBody );
        return this;
    }

    /// <summary>
    /// Adds the content types that the response will be formatted as.
    /// </summary>
    /// <param name="contentType">The response content type. Defaults to "application/json".</param>
    /// <param name="additionalContentTypes">Additional response content types the endpoint produces.</param>
    /// <returns>The original instance.</returns>
    public virtual ProducesResponseMetadataBuilder<TBuilder> FormattedAs(
        string contentType,
        params string[] additionalContentTypes )
    {
        this.contentType = contentType;
        this.additionalContentTypes = additionalContentTypes;
        return this;
    }

    /// <summary>
    /// Builds the underlying <see cref="IProducesResponseTypeMetadata"/>.
    /// </summary>
    public virtual void Build() =>
        Builder.Produces(
            StatusCode,
            responseType,
            contentType,
            additionalContentTypes ?? Array.Empty<string>() );
}