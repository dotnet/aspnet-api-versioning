// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApplicationModels;

using Microsoft.AspNetCore.Mvc.ApplicationModels;

/// <summary>
/// Represents an API controller filter that performs no filtering and includes all controllers.
/// </summary>
[CLSCompliant( false )]
public sealed class NoControllerFilter : IApiControllerFilter
{
    /// <inheritdoc />
    public IList<ControllerModel> Apply( IList<ControllerModel> controllers ) => controllers;
}