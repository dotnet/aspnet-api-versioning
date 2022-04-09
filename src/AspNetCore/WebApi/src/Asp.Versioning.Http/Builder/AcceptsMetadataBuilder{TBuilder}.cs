// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Builder;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;

/// <summary>
/// Represents a builder for <see cref="IAcceptsMetadata"/>.
/// </summary>
/// <typeparam name="TBuilder">The type of <see cref="IEndpointConventionBuilder"/>.</typeparam>
[CLSCompliant( false )]
public class AcceptsMetadataBuilder<TBuilder>
    where TBuilder : IEndpointConventionBuilder
{
    private Type? requestType;
    private string? contentType;
    private string[]? additionalContentTypes;

    /// <summary>
    /// Initializes a new instance of the <see cref="AcceptsMetadataBuilder{TBuilder}"/> class.
    /// </summary>
    /// <param name="builder">The associated endpoint builder.</param>
    /// <param name="isOptional">Sets a value that determines if the request body is optional.</param>
    public AcceptsMetadataBuilder( TBuilder builder, bool isOptional )
    {
        Builder = builder;
        IsOptional = isOptional;
    }

    /// <summary>
    /// Gets the associated endpoint builder.
    /// </summary>
    /// <value>The associated endpoint builder.</value>
    protected TBuilder Builder { get; }

    /// <summary>
    /// Gets a value indicating whether the request body is optional.
    /// </summary>
    /// <value>True if the request body is optional; otherwise, false.</value>
    protected bool IsOptional { get; }

    /// <summary>
    /// Adds the type of response that will be returned.
    /// </summary>
    /// <typeparam name="TBody">The type of request body.</typeparam>
    /// <returns>The original instance.</returns>
    public virtual AcceptsMetadataBuilder<TBuilder> Body<TBody>() where TBody : notnull
    {
        requestType = typeof( TBody );
        return this;
    }

    /// <summary>
    /// Adds the content types that the request can be formatted as.
    /// </summary>
    /// <param name="contentType">The request content type that endpoint accepts.</param>
    /// <param name="additionalContentTypes">Additional request content types the endpoint accepts.</param>
    /// <returns>The original instance.</returns>
    public virtual AcceptsMetadataBuilder<TBuilder> FormattedAs(
        string contentType,
        params string[] additionalContentTypes )
    {
        this.contentType = contentType;
        this.additionalContentTypes = additionalContentTypes;
        return this;
    }

    /// <summary>
    /// Builds the underlying <see cref="IAcceptsMetadata"/>.
    /// </summary>
    public virtual void Build() =>
        Builder.Accepts(
            requestType ?? throw new InvalidOperationException( SR.RequestTypeUnconfigured ),
            IsOptional,
            contentType ?? "application/json",
            additionalContentTypes ?? Array.Empty<string>() );
}