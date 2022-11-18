// Copyright (c) .NET Foundation and contributors. All rights reserved.

// REF: https://github.com/dotnet/aspnetcore/blob/main/src/Mvc/Mvc.Core/src/Infrastructure/DefaultProblemDetailsFactory.cs
namespace Asp.Versioning;

using Newtonsoft.Json;
using static Asp.Versioning.ProblemDetailsDefaults;
using static Newtonsoft.Json.NullValueHandling;
using static System.Globalization.CultureInfo;

internal sealed class ProblemDetailsFactory : IProblemDetailsFactory
{
    private static ProblemDetailsFactory? @default;

    public static IProblemDetailsFactory Default => @default ??= new();

    public ProblemDetails CreateProblemDetails(
        HttpRequestMessage request,
        int? statusCode = null,
        string? title = null,
        string? type = null,
        string? detail = null,
        string? instance = null )
    {
        var status = statusCode ?? 500;
        var problemDetails = new ProblemDetailsEx()
        {
            Status = status,
            Title = title,
            Type = type,
            Detail = detail,
            Instance = instance,
            TraceId = request.GetCorrelationId(),
        };

        if ( type == Ambiguous.Type )
        {
            problemDetails.Code = Ambiguous.Code;
        }
        else if ( type == Invalid.Type )
        {
            problemDetails.Code = Invalid.Code;
            return AddInvalidExtensions( request, status, problemDetails );
        }
        else if ( type == Unspecified.Type )
        {
            problemDetails.Code = Unspecified.Code;
        }
        else if ( type == Unsupported.Type )
        {
            problemDetails.Code = Unsupported.Code;
            return AddUnsupportedExtensions( request, status, problemDetails );
        }

        return problemDetails;
    }

    private static ProblemDetailsEx AddInvalidExtensions( HttpRequestMessage request, int status, ProblemDetailsEx problemDetails )
    {
        if ( status != 400 || !request.ShouldIncludeErrorDetail() )
        {
            return problemDetails;
        }

        var safeUrl = request.RequestUri.SafeFullPath();
        var requestedVersion = request.ApiVersionProperties().RawRequestedApiVersion;
        var error = string.Format( CurrentCulture, SR.VersionedControllerNameNotFound, safeUrl, requestedVersion );

        problemDetails.Error = error;

        return problemDetails;
    }

    private static ProblemDetailsEx AddUnsupportedExtensions( HttpRequestMessage request, int status, ProblemDetailsEx problemDetails )
    {
        if ( !request.ShouldIncludeErrorDetail() )
        {
            return problemDetails;
        }

        string messageFormat;

        switch ( status )
        {
            case 400:
            case 404:
                messageFormat = SR.VersionedControllerNameNotFound;
                break;
            case 405:
                messageFormat = SR.VersionedActionNameNotFound;
                break;
            default:
                return problemDetails;
        }

        var safeUrl = request.RequestUri.SafeFullPath();
        var requestedMethod = request.Method;
        var version = request.GetRequestedApiVersion()?.ToString() ?? "(null)";
        var error = string.Format( CurrentCulture, messageFormat, safeUrl, version, requestedMethod );

        problemDetails.Error = error;

        return problemDetails;
    }

    private sealed class ProblemDetailsEx : ProblemDetails
    {
        [JsonProperty( "code", NullValueHandling = Ignore )]
        public string? Code { get; set; }

        [JsonProperty( "error", NullValueHandling = Ignore )]
        public string? Error { get; set; }

        [JsonProperty( "traceId" )]
        public Guid TraceId { get; set; }
    }
}