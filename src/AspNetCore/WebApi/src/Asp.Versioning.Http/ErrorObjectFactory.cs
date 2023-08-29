// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Represents a factory that creates problem details formatted for error objects in responses.
/// </summary>
/// <remarks>This enables backward compatibility by converting <see cref="ProblemDetails"/> into Error Objects that
/// conform to the <a ref="https://github.com/microsoft/api-guidelines/blob/vNext/Guidelines.md#7102-error-condition-responses">Error Responses</a>
/// in the Microsoft REST API Guidelines and
/// <a ref="https://docs.oasis-open.org/odata/odata-json-format/v4.01/odata-json-format-v4.01.html#_Toc38457793">OData Error Responses</a>.</remarks>
[CLSCompliant( false )]
public sealed class ErrorObjectFactory : IProblemDetailsFactory
{
    private readonly IProblemDetailsFactory inner;

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorObjectFactory"/> class.
    /// </summary>
    public ErrorObjectFactory() : this( new DefaultProblemDetailsFactory() ) { }

    private ErrorObjectFactory( IProblemDetailsFactory inner ) => this.inner = inner;

    /// <summary>
    /// Creates and returns a new factory that decorates another factory.
    /// </summary>
    /// <param name="decorated">The inner, decorated factory instance.</param>
    /// <returns>A new <see cref="ErrorObjectFactory"/>.</returns>
    public static ErrorObjectFactory Decorate( IProblemDetailsFactory decorated ) => new( decorated );

    /// <inheritdoc />
    public ProblemDetails CreateProblemDetails(
        HttpRequest request,
        int? statusCode = null,
        string? title = null,
        string? type = null,
        string? detail = null,
        string? instance = null )
    {
        var problemDetails = inner.CreateProblemDetails(
            request ?? throw new ArgumentNullException( nameof( request ) ),
            statusCode,
            title,
            type,
            detail,
            instance );

        if ( IsSupported( problemDetails ) )
        {
            var response = request.HttpContext.Response;
            response.OnStarting( ChangeContentType, response );
            return ToErrorObject( problemDetails );
        }

        return problemDetails;
    }

    private static bool IsSupported( ProblemDetails problemDetails )
    {
        var type = problemDetails.Type;

        return type == ProblemDetailsDefaults.Unsupported.Type ||
               type == ProblemDetailsDefaults.Unspecified.Type ||
               type == ProblemDetailsDefaults.Invalid.Type ||
               type == ProblemDetailsDefaults.Ambiguous.Type;
    }

    private static ProblemDetails ToErrorObject( ProblemDetails problemDetails )
    {
        var error = new Dictionary<string, object>( capacity: 4 );
        var errorObject = new ProblemDetails()
        {
            Extensions =
            {
                [nameof( error )] = error,
            },
        };

        if ( !string.IsNullOrEmpty( problemDetails.Title ) )
        {
            error["message"] = problemDetails.Title;
        }

        if ( problemDetails.Extensions.TryGetValue( "code", out var value ) && value is string code )
        {
            error["code"] = code;
        }

        if ( !string.IsNullOrEmpty( problemDetails.Instance ) )
        {
            error["target"] = problemDetails.Instance;
        }

        if ( !string.IsNullOrEmpty( problemDetails.Detail ) )
        {
            error["innerError"] = new Dictionary<string, object>( capacity: 1 )
            {
                ["message"] = problemDetails.Detail,
            };
        }

        return errorObject;
    }

    private static Task ChangeContentType( object state )
    {
        var response = (HttpResponse) state;
        response.ContentType = "application/json";
        return Task.CompletedTask;
    }
}