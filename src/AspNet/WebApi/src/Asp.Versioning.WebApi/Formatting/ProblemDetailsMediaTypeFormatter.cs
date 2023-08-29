// Copyright (c) .NET Foundation and contributors. All rights reserved.

// REF: https://github.com/dotnet/aspnetcore/blob/main/src/Mvc/Mvc.Core/src/Infrastructure/DefaultProblemDetailsFactory.cs
namespace Asp.Versioning.Formatting;

using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Asp.Versioning.ProblemDetailsDefaults;

/// <summary>
/// Represents a media type formatter for problem details based on https://tools.ietf.org/html/rfc7807.
/// </summary>
public class ProblemDetailsMediaTypeFormatter : MediaTypeFormatter
{
    private readonly JsonMediaTypeFormatter json;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProblemDetailsMediaTypeFormatter"/> class.
    /// </summary>
    public ProblemDetailsMediaTypeFormatter() : this( new() ) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProblemDetailsMediaTypeFormatter"/> class.
    /// </summary>
    /// <param name="formatter">The existing instance to derive from.</param>
    public ProblemDetailsMediaTypeFormatter( JsonMediaTypeFormatter formatter )
        : base( formatter )
    {
        json = formatter;
        SupportedEncodings.Add( new UTF8Encoding( encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true ) );
        SupportedEncodings.Add( new UnicodeEncoding( bigEndian: false, byteOrderMark: true, throwOnInvalidBytes: true ) );
        SupportedMediaTypes.Add( DefaultMediaType );
    }

    /// <summary>
    /// Gets the default media type.
    /// </summary>
    /// <value>Returns the media type for application/problem+json.</value>
    public static MediaTypeHeaderValue DefaultMediaType { get; } = MediaTypeHeaderValue.Parse( MediaType.Json );

    /// <inheritdoc />
    public override bool CanReadType( Type type ) => false;

    /// <inheritdoc />
    public override bool CanWriteType( Type type ) => typeof( ProblemDetails ).IsAssignableFrom( type );

    /// <inheritdoc />
    public override Task WriteToStreamAsync(
        Type type,
        object value,
        Stream writeStream,
        HttpContent content,
        TransportContext transportContext,
        CancellationToken cancellationToken ) =>
        json.WriteToStreamAsync( type, value, writeStream, content, transportContext, cancellationToken );

    /// <inheritdoc />
    public override void SetDefaultContentHeaders( Type type, HttpContentHeaders headers, MediaTypeHeaderValue mediaType )
    {
        mediaType.MediaType = DefaultMediaType.MediaType;
        base.SetDefaultContentHeaders( type, headers, mediaType );
    }
}