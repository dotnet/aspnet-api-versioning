// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

using System.Reflection;
#if NETFRAMEWORK
using System.Web.Http.Controllers;
#endif

/// <summary>
/// Defines the behavior of a convention builder for a controller action.
/// </summary>
/// <typeparam name="T">The type of item the convention builder is for.</typeparam>
#if !NETFRAMEWORK
[CLSCompliant( false )]
#endif
public interface IActionConventionBuilder<out T> : IMapToApiVersionConventionBuilder
#if NETFRAMEWORK
    where T : notnull, IHttpController
#else
    where T : notnull
#endif
{
    /// <summary>
    /// Gets or creates a convention builder for the specified controller action method.
    /// </summary>
    /// <param name="actionMethod">The controller action <see cref="MethodInfo">method</see>
    /// to get or create a convention for.</param>
    /// <returns>A new or existing <see cref="IActionConventionBuilder{T}"/>.</returns>
    IActionConventionBuilder<T> Action( MethodInfo actionMethod );
}