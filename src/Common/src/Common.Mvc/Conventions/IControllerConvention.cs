// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

#if NETFRAMEWORK
using System.Web.Http.Controllers;
using ControllerModel = System.Web.Http.Controllers.HttpControllerDescriptor;
#else
using Microsoft.AspNetCore.Mvc.ApplicationModels;
#endif

/// <summary>
/// Defines the behavior of a controller convention.
/// </summary>
#if !NETFRAMEWORK
[CLSCompliant(false)]
#endif
public interface IControllerConvention
{
    /// <summary>
    /// Applies a controller convention given the specified builder and model.
    /// </summary>
    /// <param name="builder">The <see cref="IControllerConventionBuilder">builder</see> used to apply conventions.</param>
    /// <param name="controller">The controller to build conventions from.</param>
    /// <returns>True if any conventions were applied to the <paramref name="controller">descriptor</paramref>; otherwise, false.</returns>
    bool Apply( IControllerConventionBuilder builder, ControllerModel controller );
}