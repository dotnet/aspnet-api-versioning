// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

#if !NETFRAMEWORK
using Microsoft.AspNetCore.Mvc.ApplicationModels;
#endif
using System.Reflection;
#if NETFRAMEWORK
using ActionModel = System.Web.Http.Controllers.HttpActionDescriptor;
#endif

/// <summary>
/// Defines the behavior of a convention builder for a controller action.
/// </summary>
#if !NETFRAMEWORK
[CLSCompliant( false )]
#endif
public interface IActionConventionBuilder : IMapToApiVersionConventionBuilder, IApiVersionConvention<ActionModel>
{
    /// <summary>
    /// Gets the type of controller the convention builder is for.
    /// </summary>
    /// <value>The corresponding controller <see cref="Type">type</see>.</value>
    Type ControllerType { get; }

    /// <summary>
    /// Gets or creates a convention builder for the specified controller action method.
    /// </summary>
    /// <param name="actionMethod">The controller action <see cref="MethodInfo">method</see>
    /// to get or create a convention for.</param>
    /// <returns>A new or existing <see cref="IActionConventionBuilder"/>.</returns>
    IActionConventionBuilder Action( MethodInfo actionMethod );
}