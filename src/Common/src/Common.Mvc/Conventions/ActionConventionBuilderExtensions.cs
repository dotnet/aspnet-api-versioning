// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

#if !NETFRAMEWORK
using Microsoft.AspNetCore.Mvc;
#endif
using System.Linq.Expressions;
#if NETFRAMEWORK
using System.Web.Http;
using System.Web.Http.Controllers;
#endif

/// <summary>
/// Provides extension methods for <see cref="IActionConventionBuilder"/> and <see cref="IActionConventionBuilder{T}"/> interfaces.
/// </summary>
#if !NETFRAMEWORK
[CLSCompliant( false )]
#endif
public static class ActionConventionBuilderExtensions
{
    /// <summary>
    /// Gets or creates the convention builder for the specified controller action method.
    /// </summary>
    /// <typeparam name="TController">The type of controller.</typeparam>
    /// <param name="builder">The extended <see cref="IActionConventionBuilder{T}"/>.</param>
    /// <param name="actionExpression">The <see cref="Expression{TDelegate}">expression</see> representing the controller action method.</param>
    /// <returns>A new or existing <see cref="IActionConventionBuilder{T}"/>.</returns>
    public static IActionConventionBuilder<TController> Action<TController>(
        this IActionConventionBuilder<TController> builder,
        Expression<Action<TController>> actionExpression )
#if NETFRAMEWORK
        where TController : notnull, IHttpController
#else
        where TController : notnull
#endif
    {
        ArgumentNullException.ThrowIfNull( builder );
        ArgumentNullException.ThrowIfNull( actionExpression );
        return builder.Action( actionExpression.ExtractMethod() );
    }

    /// <summary>
    /// Gets or creates the convention builder for the specified controller action method.
    /// </summary>
    /// <typeparam name="TController">The type of controller.</typeparam>
    /// <typeparam name="TResult">The type of action result.</typeparam>
    /// <param name="builder">The extended <see cref="IActionConventionBuilder{T}"/>.</param>
    /// <param name="actionExpression">The <see cref="Expression{TDelegate}">expression</see> representing the controller action method.</param>
    /// <returns>A new or existing <see cref="IActionConventionBuilder{T}"/>.</returns>
    public static IActionConventionBuilder<TController> Action<TController, TResult>(
        this IActionConventionBuilder<TController> builder,
        Expression<Func<TController, TResult>> actionExpression )
#if NETFRAMEWORK
        where TController : notnull, IHttpController
#else
        where TController : notnull
#endif
    {
        ArgumentNullException.ThrowIfNull( builder );
        ArgumentNullException.ThrowIfNull( actionExpression );
        return builder.Action( actionExpression.ExtractMethod() );
    }

    /// <summary>
    /// Gets or creates the convention builder for the specified controller action method.
    /// </summary>
    /// <param name="builder">The extended <see cref="ActionApiVersionConventionBuilder"/>.</param>
    /// <param name="methodName">The name of the action method.</param>
    /// <param name="argumentTypes">The optional array of action method argument types.</param>
    /// <returns>A new or existing <see cref="ActionApiVersionConventionBuilder"/>.</returns>
    /// <remarks>The specified <paramref name="methodName">method name</paramref> must refer to a public, non-static action method.
    /// If there is only one corresponding match found, then the <paramref name="argumentTypes">argument types</paramref> are ignored;
    /// otherwise, the <paramref name="argumentTypes">argument types</paramref> are used for method overload resolution. Action
    /// methods that have the <see cref="NonActionAttribute"/> applied will also be ignored.</remarks>
    public static IActionConventionBuilder Action( this IActionConventionBuilder builder, string methodName, params Type[] argumentTypes )
    {
        ArgumentNullException.ThrowIfNull( builder );
        var method = ActionMethodResolver.Resolve( builder.ControllerType, methodName, argumentTypes );
        return builder.Action( method );
    }
}