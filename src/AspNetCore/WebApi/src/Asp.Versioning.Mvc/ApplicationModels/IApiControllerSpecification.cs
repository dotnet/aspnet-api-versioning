﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApplicationModels;

using Microsoft.AspNetCore.Mvc.ApplicationModels;

/// <summary>
/// Defines the behavior of an API controller specification.
/// </summary>
[CLSCompliant( false )]
public interface IApiControllerSpecification
{
    /// <summary>
    /// Determines whether the specification is satisfied by the provided controller model.
    /// </summary>
    /// <param name="controller">The <see cref="ControllerModel">controller model</see> to evaluate.</param>
    /// <returns>True if the <paramref name="controller"/> satisfies the specification; otherwise, false.</returns>
    bool IsSatisfiedBy( ControllerModel controller );
}