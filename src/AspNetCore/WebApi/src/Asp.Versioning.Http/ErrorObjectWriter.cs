// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;
using static System.Text.Json.Serialization.JsonIgnoreCondition;
using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

/// <summary>
/// Represents a problem details writer that outputs error objects in responses.
/// </summary>
/// <remarks>This enables backward compatibility by converting <see cref="ProblemDetails"/> into Error Objects that
/// conform to the <a ref="https://github.com/microsoft/api-guidelines/blob/vNext/Guidelines.md#7102-error-condition-responses">Error Responses</a>
/// in the Microsoft REST API Guidelines and
/// <a ref="https://docs.oasis-open.org/odata/odata-json-format/v4.01/odata-json-format-v4.01.html#_Toc38457793">OData Error Responses</a>.</remarks>
[CLSCompliant( false )]
public partial class ErrorObjectWriter : IProblemDetailsWriter
{
    private readonly JsonSerializerOptions options;

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorObjectWriter"/> class.
    /// </summary>
    /// <param name="options">The current <see cref="JsonOptions">JSON options</see>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <c>null</c>.</exception>
    public ErrorObjectWriter( IOptions<JsonOptions> options ) =>
        this.options = ( options ?? throw new ArgumentNullException( nameof( options ) ) ).Value.SerializerOptions;

    /// <inheritdoc />
    public virtual bool CanWrite( ProblemDetailsContext context )
    {
        ArgumentNullException.ThrowIfNull( context );

        var type = context.ProblemDetails.Type;

        return type == ProblemDetailsDefaults.Unsupported.Type ||
               type == ProblemDetailsDefaults.Unspecified.Type ||
               type == ProblemDetailsDefaults.Invalid.Type ||
               type == ProblemDetailsDefaults.Ambiguous.Type;
    }

    /// <inheritdoc />
    public virtual ValueTask WriteAsync( ProblemDetailsContext context )
    {
        ArgumentNullException.ThrowIfNull( context );

        var response = context.HttpContext.Response;
        var obj = new ErrorObject( context.ProblemDetails );

        OnBeforeWrite( context, ref obj );

        return new( response.WriteAsJsonAsync( obj, options.GetTypeInfo( obj.GetType() ) ) );
    }

    /// <summary>
    /// Occurs just before an error will be written.
    /// </summary>
    /// <param name="context">The current context.</param>
    /// <param name="errorObject">The current error object.</param>
    /// <remarks><b>Note to inheritors: </b>The default implementation performs no action.</remarks>
    protected virtual void OnBeforeWrite( ProblemDetailsContext context, ref ErrorObject errorObject )
    {
    }

#pragma warning disable CA1815 // Override equals and operator equals on value types

    /// <summary>
    /// Represents an error object.
    /// </summary>
    protected internal readonly partial struct ErrorObject
    {
        internal ErrorObject( ProblemDetails problemDetails ) =>
            Error = new( problemDetails );

        /// <summary>
        /// Gets the top-level error.
        /// </summary>
        /// <value>The top-level error.</value>
        [JsonPropertyName( "error" )]
        public ErrorDetail Error { get; }
    }

    /// <summary>
    /// Represents the error detail.
    /// </summary>
    protected internal readonly partial struct ErrorDetail
    {
        private readonly ProblemDetails problemDetails;
        private readonly InnerError? innerError;
        private readonly Dictionary<string, object> extensions = [];

        internal ErrorDetail( ProblemDetails problemDetails )
        {
            this.problemDetails = problemDetails;
            innerError = string.IsNullOrEmpty( problemDetails.Detail ) ? default : new InnerError( problemDetails );
        }

        /// <summary>
        /// Gets or sets one of a server-defined set of error codes.
        /// </summary>
        /// <value>A server-defined error code.</value>
        [JsonPropertyName( "code" )]
        [JsonIgnore( Condition = WhenWritingNull )]
        public string? Code
        {
            get => problemDetails.Extensions.TryGetValue( "code", out var value ) &&
                   value is string code ?
                   code :
                   default;
            set
            {
                if ( value is null )
                {
                    problemDetails.Extensions.Remove( "code" );
                }
                else
                {
                    problemDetails.Extensions["code"] = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        /// <value>A human-readable representation of the error.</value>
        [JsonPropertyName( "message" )]
        [JsonIgnore( Condition = WhenWritingNull )]
        public string? Message
        {
            get => problemDetails.Title;
            set => problemDetails.Title = value;
        }

        /// <summary>
        /// Gets or sets the target of the error.
        /// </summary>
        /// <value>The error target of the error.</value>
        [JsonPropertyName( "target" )]
        [JsonIgnore( Condition = WhenWritingNull )]
        public string? Target
        {
            get => problemDetails.Title;
            set => problemDetails.Title = value;
        }

        /// <summary>
        /// Gets an object containing more specific information than the current object about the error, if any.
        /// </summary>
        /// <value>The inner error or <c>null</c>.</value>
        [JsonPropertyName( "innerError" )]
        [JsonIgnore( Condition = WhenWritingNull )]
        public InnerError? InnerError => innerError;

        /// <summary>
        /// Gets a collection of extension key/value pair members.
        /// </summary>
        /// <value>A collection of extension key/value pair members.</value>
        [JsonExtensionData]
        public IDictionary<string, object> Extensions => extensions;
    }

    /// <summary>
    /// Represents an inner error.
    /// </summary>
    protected internal readonly partial struct InnerError
    {
        private readonly ProblemDetails problemDetails;
        private readonly Dictionary<string, object> extensions = [];

        internal InnerError( ProblemDetails problemDetails ) =>
            this.problemDetails = problemDetails;

        /// <summary>
        /// Gets or sets the inner error message.
        /// </summary>
        /// <value>The inner error message.</value>
        [JsonPropertyName( "message" )]
        [JsonIgnore( Condition = WhenWritingNull )]
        public string? Message
        {
            get => problemDetails.Detail;
            set => problemDetails.Detail = value;
        }

        /// <summary>
        /// Gets a collection of extension key/value pair members.
        /// </summary>
        /// <value>A collection of extension key/value pair members.</value>
        [JsonExtensionData]
        public IDictionary<string, object> Extensions => extensions;
    }

    [JsonSerializable( typeof( ErrorObject ) )]
    internal sealed partial class ErrorObjectJsonContext : JsonSerializerContext
    {
    }
}