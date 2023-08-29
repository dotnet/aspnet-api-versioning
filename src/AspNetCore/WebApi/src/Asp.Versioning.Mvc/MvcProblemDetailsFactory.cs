// Copyright (c) .NET Foundation and contributors. All rights reserved.

// Ignore Spelling: Mvc
#pragma warning disable CA1812

namespace Asp.Versioning;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

internal sealed class MvcProblemDetailsFactory : IProblemDetailsFactory
{
    private readonly ProblemDetailsFactory factory;

    public MvcProblemDetailsFactory( ProblemDetailsFactory factory ) => this.factory = factory;

    public ProblemDetails CreateProblemDetails(
        HttpRequest request,
        int? statusCode = null,
        string? title = null,
        string? type = null,
        string? detail = null,
        string? instance = null )
    {
        var httpContext = request.HttpContext;
        var problemDetails = factory.CreateProblemDetails( httpContext, statusCode, title, type, detail, instance );
        DefaultProblemDetailsFactory.ApplyExtensions( problemDetails );
        return problemDetails;
    }
}