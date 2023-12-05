// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0079
#pragma warning disable SA1001

namespace Asp.Versioning.Conventions;

#if !NETFRAMEWORK
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Attributes;
#endif
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
#if NETFRAMEWORK
using System.Web.Http;
using System.Web.Http.Controllers;
#endif

/// <summary>
/// Provides extension methods for the <see cref="ODataActionQueryOptionsConventionBuilder"/>
/// and <see cref="ODataActionQueryOptionsConventionBuilder{T}"/> class.
/// </summary>
#if !NETFRAMEWORK
[CLSCompliant( false )]
#endif
public static class ODataActionQueryOptionsConventionBuilderExtensions
{
    /// <summary>
    /// Allows the $orderby query option.
    /// </summary>
    /// <param name="builder">The extended convention builder.</param>
    /// <param name="maxNodeCount">The maximum number of expressions in the $orderby query option or zero to indicate the default.</param>
    /// <param name="properties">The array of property names that can appear in the $orderby query option.
    /// An empty array indicates that any property can appear in the $orderby query option.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static ODataActionQueryOptionsConventionBuilder AllowOrderBy(
        this ODataActionQueryOptionsConventionBuilder builder,
        int maxNodeCount,
        params string[] properties )
    {
        ArgumentNullException.ThrowIfNull( builder );
        return builder.AllowOrderBy( maxNodeCount, properties.AsEnumerable() );
    }

    /// <summary>
    /// Allows the $orderby query option.
    /// </summary>
    /// <param name="builder">The extended convention builder.</param>
    /// <param name="properties">The <see cref="IEnumerable{T}">sequence</see> of property names that can appear in the $orderby query option.
    /// An empty sequence indicates that any property can appear in the $orderby query option.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static ODataActionQueryOptionsConventionBuilder AllowOrderBy(
        this ODataActionQueryOptionsConventionBuilder builder,
        IEnumerable<string> properties )
    {
        ArgumentNullException.ThrowIfNull( builder );
        return builder.AllowOrderBy( default, properties );
    }

    /// <summary>
    /// Allows the $orderby query option.
    /// </summary>
    /// <param name="builder">The extended convention builder.</param>
    /// <param name="properties">The array of property names that can appear in the $orderby query option.
    /// An empty array indicates that any property can appear in the $orderby query option.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static ODataActionQueryOptionsConventionBuilder AllowOrderBy(
        this ODataActionQueryOptionsConventionBuilder builder,
        params string[] properties )
    {
        ArgumentNullException.ThrowIfNull( builder );
        return builder.AllowOrderBy( default, properties.AsEnumerable() );
    }

    /// <summary>
    /// Allows the $orderby query option.
    /// </summary>
    /// <typeparam name="T">The type of controller.</typeparam>
    /// <param name="builder">The extended convention builder.</param>
    /// <param name="maxNodeCount">The maximum number of expressions in the $orderby query option or zero to indicate the default.</param>
    /// <param name="properties">The array of property names that can appear in the $orderby query option.
    /// An empty array indicates that any property can appear in the $orderby query option.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static ODataActionQueryOptionsConventionBuilder<T> AllowOrderBy<T>(
        this ODataActionQueryOptionsConventionBuilder<T> builder,
        int maxNodeCount,
        params string[] properties )
        where T : notnull
#if NETFRAMEWORK
        , IHttpController
#endif
    {
        ArgumentNullException.ThrowIfNull( builder );
        return builder.AllowOrderBy( maxNodeCount, properties.AsEnumerable() );
    }

    /// <summary>
    /// Allows the $orderby query option.
    /// </summary>
    /// <typeparam name="T">The type of controller.</typeparam>
    /// <param name="builder">The extended convention builder.</param>
    /// <param name="properties">The <see cref="IEnumerable{T}">sequence</see> of property names that can appear in the $orderby query option.
    /// An empty sequence indicates that any property can appear in the $orderby query option.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static ODataActionQueryOptionsConventionBuilder<T> AllowOrderBy<T>(
        this ODataActionQueryOptionsConventionBuilder<T> builder,
        IEnumerable<string> properties )
        where T : notnull
#if NETFRAMEWORK
        , IHttpController
#endif
    {
        ArgumentNullException.ThrowIfNull( builder );
        return builder.AllowOrderBy( default, properties );
    }

    /// <summary>
    /// Allows the $orderby query option.
    /// </summary>
    /// <typeparam name="T">The type of controller.</typeparam>
    /// <param name="builder">The extended convention builder.</param>
    /// <param name="properties">The array of property names that can appear in the $orderby query option.
    /// An empty array indicates that any property can appear in the $orderby query option.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static ODataActionQueryOptionsConventionBuilder<T> AllowOrderBy<T>(
        this ODataActionQueryOptionsConventionBuilder<T> builder,
        params string[] properties )
        where T : notnull
#if NETFRAMEWORK
        , IHttpController
#endif
    {
        ArgumentNullException.ThrowIfNull( builder );
        return builder.AllowOrderBy( default, properties.AsEnumerable() );
    }

    /// <summary>
    /// Gets or creates the convention builder for the specified controller action method.
    /// </summary>
    /// <typeparam name="TController">The type of controller.</typeparam>
    /// <param name="builder">The extended <see cref="IODataActionQueryOptionsConventionBuilder{T}"/>.</param>
    /// <param name="actionExpression">The <see cref="Expression{TDelegate}">expression</see> representing the controller action method.</param>
    /// <returns>A new or existing <see cref="ODataActionQueryOptionsConventionBuilder{T}"/>.</returns>
    public static ODataActionQueryOptionsConventionBuilder<TController> Action<TController>(
        this IODataActionQueryOptionsConventionBuilder<TController> builder,
        Expression<Action<TController>> actionExpression )
        where TController : notnull
#if NETFRAMEWORK
       , IHttpController
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
    /// <param name="builder">The extended <see cref="IODataActionQueryOptionsConventionBuilder{T}"/>.</param>
    /// <param name="actionExpression">The <see cref="Expression{TDelegate}">expression</see> representing the controller action method.</param>
    /// <returns>A new or existing <see cref="ODataActionQueryOptionsConventionBuilder{T}"/>.</returns>
    public static ODataActionQueryOptionsConventionBuilder<TController> Action<TController, TResult>(
        this IODataActionQueryOptionsConventionBuilder<TController> builder,
        Expression<Func<TController, TResult>> actionExpression )
        where TController : notnull
#if NETFRAMEWORK
       , IHttpController
#endif
    {
        ArgumentNullException.ThrowIfNull( builder );
        ArgumentNullException.ThrowIfNull( actionExpression );
        return builder.Action( actionExpression.ExtractMethod() );
    }

    /// <summary>
    /// Gets or creates the convention builder for the specified controller action method.
    /// </summary>
    /// <param name="builder">The extended convention builder.</param>
    /// <param name="methodName">The name of the action method.</param>
    /// <param name="argumentTypes">The optional array of action method argument types.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    /// <remarks>The specified <paramref name="methodName">method name</paramref> must refer to a public, non-static action method.
    /// If there is only one corresponding match found, then the <paramref name="argumentTypes">argument types</paramref> are ignored;
    /// otherwise, the <paramref name="argumentTypes">argument types</paramref> are used for method overload resolution. Action
    /// methods that have the <see cref="NonActionAttribute"/> applied will also be ignored.</remarks>
    public static ODataActionQueryOptionsConventionBuilder Action(
        this IODataActionQueryOptionsConventionBuilder builder,
        string methodName,
        params Type[] argumentTypes )
    {
        ArgumentNullException.ThrowIfNull( builder );
        ArgumentNullException.ThrowIfNull( argumentTypes );

        string message;
        var methods = builder.ControllerType
                             .GetRuntimeMethods()
                             .Where( m => m.Name == methodName && IsAction( m ) )
                             .ToArray();

        switch ( methods.Length )
        {
            case 0:
                message = string.Format( CultureInfo.CurrentCulture, Format.ActionMethodNotFound, methodName );
                throw new MissingMethodException( message );
            case 1:
                return builder.Action( methods[0] );
        }

        // perf: stop if there are 2+ matches; it's ambiguous
        methods = methods.Where( m => SignatureMatches( m, argumentTypes ) ).Take( 2 ).ToArray();

        if ( methods.Length == 1 )
        {
            return builder.Action( methods[0] );
        }

        message = string.Format( CultureInfo.CurrentCulture, Format.AmbiguousActionMethod, methodName );
        throw new AmbiguousMatchException( message );
    }

    private static bool IsAction( MethodInfo method )
    {
        if ( !method.IsPublic || method.IsStatic )
        {
            return false;
        }

        return method.GetCustomAttribute<NonActionAttribute>() == null
#if !NETFRAMEWORK
               && method.GetCustomAttribute<ODataIgnoredAttribute>() == null
#endif
        ;
    }

    private static bool SignatureMatches( MethodInfo method, Type[] argumentTypes )
    {
        var parameters = method.GetParameters();

        if ( parameters.Length != argumentTypes.Length )
        {
            return false;
        }

        for ( var i = 0; i < parameters.Length; i++ )
        {
            if ( parameters[i].ParameterType != argumentTypes[i] )
            {
                return false;
            }
        }

        return true;
    }

    private static MethodInfo ExtractMethod<TDelegate>( this Expression<TDelegate> expression )
    {
        if ( expression.Body is MethodCallExpression methodCall )
        {
            return methodCall.Method;
        }

        var message = string.Format( CultureInfo.CurrentCulture, Format.InvalidActionMethodExpression, expression );
        throw new InvalidOperationException( message );
    }
}