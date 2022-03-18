// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable CA1812

// REF: https://github.com/dotnet/aspnetcore/blob/main/src/Mvc/Mvc.Core/src/Infrastructure/DefaultProblemDetailsFactory.cs
namespace Asp.Versioning;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using static Asp.Versioning.ProblemDetailsDefaults;

/// <summary>
/// Represents the default <see cref="IProblemDetailsFactory">problem details factory</see>.
/// </summary>
[CLSCompliant( false )]
public sealed class DefaultProblemDetailsFactory : IProblemDetailsFactory
{
    /// <inheritdoc />
    public ProblemDetails CreateProblemDetails(
        HttpRequest request,
        int? statusCode = null,
        string? title = null,
        string? type = null,
        string? detail = null,
        string? instance = null )
    {
        var problemDetails = new ProblemDetails()
        {
            Status = statusCode ?? 500,
            Title = title,
            Type = type,
            Detail = detail,
            Instance = instance,
        };

        ApplyExtensions( problemDetails );

        if ( ( Activity.Current?.Id ?? request?.HttpContext.TraceIdentifier ) is string traceId )
        {
            problemDetails.Extensions[nameof( traceId )] = traceId;
        }

        return problemDetails;
    }

    /// <summary>
    /// Applies the default extensions to the specified problem details.
    /// </summary>
    /// <param name="problemDetails">The problem details to apply the extensions to.</param>
    /// <exception cref="ArgumentNullException"><paramref name="problemDetails"/> is <c>null</c>.</exception>
    public static void ApplyExtensions( ProblemDetails problemDetails )
    {
        const string Code = nameof( Code );
        var type = ( problemDetails ?? throw new ArgumentNullException( nameof( problemDetails ) ) ).Type;

        if ( type == Ambiguous.Type )
        {
            problemDetails.Extensions[Code] = Ambiguous.Code;
        }
        else if ( type == Invalid.Type )
        {
            problemDetails.Extensions[Code] = Invalid.Code;
        }
        else if ( type == Unspecified.Type )
        {
            problemDetails.Extensions[Code] = Unspecified.Code;
        }
        else if ( type == Unsupported.Type )
        {
            problemDetails.Extensions[Code] = Unsupported.Code;
        }
    }
}