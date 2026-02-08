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
using static System.Reflection.BindingFlags;

/// <summary>
/// Provides extension methods for the <see cref="ODataActionQueryOptionsConventionBuilder"/>
/// and <see cref="ODataActionQueryOptionsConventionBuilder{T}"/> class.
/// </summary>
#if !NETFRAMEWORK
[CLSCompliant( false )]
#endif
public static class ODataActionQueryOptionsConventionBuilderExtensions
{
    /// <param name="builder">The extended convention builder.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    extension( ODataActionQueryOptionsConventionBuilder builder )
    {
        /// <summary>
        /// Allows the $orderby query option.
        /// </summary>
        /// <param name="maxNodeCount">The maximum number of expressions in the $orderby query option or zero to indicate the default.</param>
        /// <param name="properties">The array of property names that can appear in the $orderby query option.
        /// An empty array indicates that any property can appear in the $orderby query option.</param>
        public ODataActionQueryOptionsConventionBuilder AllowOrderBy( int maxNodeCount, params string[] properties )
        {
            ArgumentNullException.ThrowIfNull( builder );
            return builder.AllowOrderBy( maxNodeCount, properties.AsEnumerable() );
        }

        /// <summary>
        /// Allows the $orderby query option.
        /// </summary>
        /// <param name="properties">The <see cref="IEnumerable{T}">sequence</see> of property names that can appear in the $orderby query option.
        /// An empty sequence indicates that any property can appear in the $orderby query option.</param>
        public ODataActionQueryOptionsConventionBuilder AllowOrderBy( IEnumerable<string> properties )
        {
            ArgumentNullException.ThrowIfNull( builder );
            return builder.AllowOrderBy( default, properties );
        }

        /// <summary>
        /// Allows the $orderby query option.
        /// </summary>
        /// <param name="properties">The array of property names that can appear in the $orderby query option.
        /// An empty array indicates that any property can appear in the $orderby query option.</param>
        public ODataActionQueryOptionsConventionBuilder AllowOrderBy( params string[] properties )
        {
            ArgumentNullException.ThrowIfNull( builder );
            return builder.AllowOrderBy( default, properties.AsEnumerable() );
        }
    }

    /// <typeparam name="T">The type of controller.</typeparam>
    /// <param name="builder">The extended convention builder.</param>
    extension<T>( ODataActionQueryOptionsConventionBuilder<T> builder )
        where T : notnull
#if NETFRAMEWORK
            , IHttpController
#endif
    {
        /// <summary>
        /// Allows the $orderby query option.
        /// </summary>
        /// <param name="maxNodeCount">The maximum number of expressions in the $orderby query option or zero to indicate the default.</param>
        /// <param name="properties">The array of property names that can appear in the $orderby query option.
        /// An empty array indicates that any property can appear in the $orderby query option.</param>
        public ODataActionQueryOptionsConventionBuilder<T> AllowOrderBy( int maxNodeCount, params string[] properties )
        {
            ArgumentNullException.ThrowIfNull( builder );
            return builder.AllowOrderBy( maxNodeCount, properties.AsEnumerable() );
        }

        /// <summary>
        /// Allows the $orderby query option.
        /// </summary>
        /// <param name="properties">The <see cref="IEnumerable{T}">sequence</see> of property names that can appear in the $orderby query option.
        /// An empty sequence indicates that any property can appear in the $orderby query option.</param>
        public ODataActionQueryOptionsConventionBuilder<T> AllowOrderBy( IEnumerable<string> properties )
        {
            ArgumentNullException.ThrowIfNull( builder );
            return builder.AllowOrderBy( default, properties );
        }

        /// <summary>
        /// Allows the $orderby query option.
        /// </summary>
        /// <param name="properties">The array of property names that can appear in the $orderby query option.
        /// An empty array indicates that any property can appear in the $orderby query option.</param>
        public ODataActionQueryOptionsConventionBuilder<T> AllowOrderBy( params string[] properties )
        {
            ArgumentNullException.ThrowIfNull( builder );
            return builder.AllowOrderBy( default, properties.AsEnumerable() );
        }
    }

    /// <typeparam name="TController">The type of controller.</typeparam>
    /// <param name="builder">The extended <see cref="IODataActionQueryOptionsConventionBuilder{T}"/>.</param>
    extension<TController>( IODataActionQueryOptionsConventionBuilder<TController> builder )
        where TController : notnull
#if NETFRAMEWORK
       , IHttpController
#endif
    {
        /// <summary>
        /// Gets or creates the convention builder for the specified controller action method.
        /// </summary>
        /// <param name="actionExpression">The <see cref="Expression{TDelegate}">expression</see> representing the controller action method.</param>
        /// <returns>A new or existing <see cref="ODataActionQueryOptionsConventionBuilder{T}"/>.</returns>
        public ODataActionQueryOptionsConventionBuilder<TController> Action( Expression<Action<TController>> actionExpression )
        {
            ArgumentNullException.ThrowIfNull( builder );
            ArgumentNullException.ThrowIfNull( actionExpression );
            return builder.Action( actionExpression.ExtractMethod() );
        }

        /// <summary>
        /// Gets or creates the convention builder for the specified controller action method.
        /// </summary>
        /// <typeparam name="TResult">The type of action result.</typeparam>
        /// <param name="actionExpression">The <see cref="Expression{TDelegate}">expression</see> representing the controller action method.</param>
        /// <returns>A new or existing <see cref="ODataActionQueryOptionsConventionBuilder{T}"/>.</returns>
        public ODataActionQueryOptionsConventionBuilder<TController> Action<TResult>( Expression<Func<TController, TResult>> actionExpression )
        {
            ArgumentNullException.ThrowIfNull( builder );
            ArgumentNullException.ThrowIfNull( actionExpression );
            return builder.Action( actionExpression.ExtractMethod() );
        }
    }

    /// <param name="builder">The extended convention builder.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    extension( IODataActionQueryOptionsConventionBuilder builder )
    {
        /// <summary>
        /// Gets or creates the convention builder for the specified controller action method.
        /// </summary>
        /// <param name="methodName">The name of the action method.</param>
        /// <param name="argumentTypes">The optional array of action method argument types.</param>
        /// <remarks>The specified <paramref name="methodName">method name</paramref> must refer to a public, non-action method.
        /// If there is only one corresponding match found, then the <paramref name="argumentTypes">argument types</paramref> are ignored;
        /// otherwise, the <paramref name="argumentTypes">argument types</paramref> are used for method overload resolution. Action
        /// methods that have the <see cref="NonActionAttribute"/> applied will also be ignored.</remarks>
#if !NETFRAMEWORK
        [UnconditionalSuppressMessage( "ILLink", "IL2075", Justification = "Controller types and actions are never trimmed" )]
#endif
        public ODataActionQueryOptionsConventionBuilder Action( string methodName, params Type[] argumentTypes )
        {
            ArgumentNullException.ThrowIfNull( builder );
            ArgumentNullException.ThrowIfNull( argumentTypes );

            string message;
            var methods = builder.ControllerType
                                 .GetMethods( Instance | Public )
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
            methods = [.. methods.Where( m => SignatureMatches( m, argumentTypes ) ).Take( 2 )];

            if ( methods.Length == 1 )
            {
                return builder.Action( methods[0] );
            }

            message = string.Format( CultureInfo.CurrentCulture, Format.AmbiguousActionMethod, methodName );
            throw new AmbiguousMatchException( message );
        }
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