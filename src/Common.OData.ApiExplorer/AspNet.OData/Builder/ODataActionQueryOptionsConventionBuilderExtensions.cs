namespace Microsoft.AspNet.OData.Builder
{
#if !WEBAPI
    using Microsoft.AspNetCore.Mvc;
#endif
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
#if WEBAPI
    using System.Web.Http;
    using System.Web.Http.Controllers;
#endif

    /// <summary>
    /// Provides extension methods for the <see cref="ODataActionQueryOptionsConventionBuilder"/> and <see cref="ODataActionQueryOptionsConventionBuilder{T}"/> class.
    /// </summary>
    public static partial class ODataActionQueryOptionsConventionBuilderExtensions
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
            if ( builder == null )
            {
                throw new ArgumentNullException( nameof( builder ) );
            }

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
            if ( builder == null )
            {
                throw new ArgumentNullException( nameof( builder ) );
            }

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
            if ( builder == null )
            {
                throw new ArgumentNullException( nameof( builder ) );
            }

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
#if WEBAPI
#pragma warning disable SA1001 // Commas should be spaced correctly
       , IHttpController
#pragma warning restore SA1001 // Commas should be spaced correctly
#endif
        {
            if ( builder == null )
            {
                throw new ArgumentNullException( nameof( builder ) );
            }

            if ( actionExpression == null )
            {
                throw new ArgumentNullException( nameof( actionExpression ) );
            }

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
#if WEBAPI
#pragma warning disable SA1001 // Commas should be spaced correctly
       , IHttpController
#pragma warning restore SA1001 // Commas should be spaced correctly
#endif
        {
            if ( builder == null )
            {
                throw new ArgumentNullException( nameof( builder ) );
            }

            if ( actionExpression == null )
            {
                throw new ArgumentNullException( nameof( actionExpression ) );
            }

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
        public static ODataActionQueryOptionsConventionBuilder Action( this IODataActionQueryOptionsConventionBuilder builder, string methodName, params Type[] argumentTypes )
        {
            if ( builder == null )
            {
                throw new ArgumentNullException( nameof( builder ) );
            }

            var methods = builder.ControllerType.GetRuntimeMethods().Where( m => m.Name == methodName && IsAction( m ) ).ToArray();

            switch ( methods.Length )
            {
                case 0:
                    throw new MissingMethodException( SR.ActionMethodNotFound.FormatDefault( methodName ) );
                case 1:
                    return builder.Action( methods[0] );
            }

            argumentTypes ??= Type.EmptyTypes;
            methods = methods.Where( m => SignatureMatches( m, argumentTypes ) ).ToArray();

            if ( methods.Length == 1 )
            {
                return builder.Action( methods[0] );
            }

            throw new AmbiguousMatchException( SR.AmbiguousActionMethod.FormatDefault( methodName ) );
        }

        static bool IsAction( MethodInfo method )
        {
            if ( !method.IsPublic || method.IsStatic )
            {
                return false;
            }

            return method.GetCustomAttribute<NonActionAttribute>() == null;
        }

        static bool SignatureMatches( MethodInfo method, Type[] argumentTypes )
        {
            var argTypes = method.GetParameters().Select( p => p.ParameterType ).ToArray();
            return argTypes.SequenceEqual( argumentTypes );
        }

        static MethodInfo ExtractMethod<TDelegate>( this Expression<TDelegate> expression )
        {
            if ( expression.Body is MethodCallExpression methodCall )
            {
                return methodCall.Method;
            }

            throw new InvalidOperationException( SR.InvalidActionMethodExpression.FormatDefault( expression ) );
        }
    }
}