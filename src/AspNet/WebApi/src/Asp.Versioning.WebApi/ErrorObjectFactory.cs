// Copyright (c) .NET Foundation and contributors. All rights reserved.

// REF: https://github.com/dotnet/aspnetcore/blob/main/src/Mvc/Mvc.Core/src/Infrastructure/DefaultProblemDetailsFactory.cs
namespace Asp.Versioning;

using Newtonsoft.Json;
using static Asp.Versioning.ProblemDetailsDefaults;
using static Newtonsoft.Json.NullValueHandling;

internal sealed class ErrorObjectFactory : IProblemDetailsFactory
{
    public ProblemDetails CreateProblemDetails(
        HttpRequestMessage request,
        int? statusCode = null,
        string? title = null,
        string? type = null,
        string? detail = null,
        string? instance = null )
    {
        var status = statusCode ?? 500;
        ErrorObject? problem;

        if ( type == Ambiguous.Type )
        {
            problem = NewError( title, instance );
            problem.Error.Code = Ambiguous.Code;
        }
        else if ( type == Invalid.Type )
        {
            problem = NewError( title, instance );
            problem.Error.Code = Invalid.Code;
            return ProblemDetailsFactory.AddInvalidExtensions( request, status, problem, ApplyMessage );
        }
        else if ( type == Unspecified.Type )
        {
            problem = NewError( title, instance );
            problem.Error.Code = Unspecified.Code;
        }
        else if ( type == Unsupported.Type )
        {
            problem = NewError( title, instance );
            problem.Error.Code = Unsupported.Code;
            return ProblemDetailsFactory.AddUnsupportedExtensions( request, status, problem, ApplyMessage );
        }

        return ProblemDetailsFactory.NewProblemDetails(
            request,
            statusCode,
            title,
            type,
            detail,
            instance );
    }

    private static ErrorObject NewError( string? message, string? target ) =>
        new()
        {
            Error =
            {
                Message = message,
                Target = target,
            },
        };

    private static void ApplyMessage( ErrorObject obj, string message ) =>
        obj.Error.InnerError = new() { Message = message };

    private sealed class ErrorObject : ProblemDetails
    {
        [JsonProperty( "error" )]
        public ErrorDetail Error { get; } = new();
    }

    private sealed class ErrorDetail
    {
        [JsonProperty( "code", NullValueHandling = Ignore )]
        public string? Code { get; set; }

        [JsonProperty( "message", NullValueHandling = Ignore )]
        public string? Message { get; set; }

        [JsonProperty( "target", NullValueHandling = Ignore )]
        public string? Target { get; set; }

        [JsonProperty( "innerError", NullValueHandling = Ignore )]
        public InnerError? InnerError { get; set; }
    }

    private sealed class InnerError
    {
        [JsonProperty( "message", NullValueHandling = Ignore )]
        public string? Message { get; set; }
    }
}