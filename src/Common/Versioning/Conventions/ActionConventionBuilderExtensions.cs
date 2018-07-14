#if WEBAPI
namespace Microsoft.Web.Http.Versioning.Conventions
#else
namespace Microsoft.AspNetCore.Mvc.Versioning.Conventions
#endif
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
#if WEBAPI
    using System.Web.Http;
    using System.Web.Http.Controllers;
#endif

    /// <summary>
    /// Provides extension methods for <see cref="IActionConventionBuilder"/> and <see cref="IActionConventionBuilder{T}"/> interfaces.
    /// </summary>
#if !WEBAPI
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
        /// <returns>A new or existing <see cref="ActionApiVersionConventionBuilder{T}"/>.</returns>
        public static ActionApiVersionConventionBuilder<TController> Action<TController>( this IActionConventionBuilder<TController> builder, Expression<Action<TController>> actionExpression )
#if WEBAPI
            where TController : IHttpController
#endif
        {
            Arg.NotNull( builder, nameof( builder ) );
            Arg.NotNull( actionExpression, nameof( actionExpression ) );
            return builder.Action( actionExpression.ExtractMethod() );
        }

        /// <summary>
        /// Gets or creates the convention builder for the specified controller action method.
        /// </summary>
        /// <typeparam name="TController">The type of controller.</typeparam>
        /// <typeparam name="TResult">The type of action result.</typeparam>
        /// <param name="builder">The extended <see cref="IActionConventionBuilder{T}"/>.</param>
        /// <param name="actionExpression">The <see cref="Expression{TDelegate}">expression</see> representing the controller action method.</param>
        /// <returns>A new or existing <see cref="ActionApiVersionConventionBuilder{T}"/>.</returns>
        public static ActionApiVersionConventionBuilder<TController> Action<TController, TResult>( this IActionConventionBuilder<TController> builder, Expression<Func<TController, TResult>> actionExpression )
#if WEBAPI
            where TController : IHttpController
#endif
        {
            Arg.NotNull( builder, nameof( builder ) );
            Arg.NotNull( actionExpression, nameof( actionExpression ) );
            return builder.Action( actionExpression.ExtractMethod() );
        }

        /// <summary>
        /// Gets or creates the convention builder for the specified controller action method.
        /// </summary>
        /// <param name="builder">The extended <see cref="IActionConventionBuilder"/>.</param>
        /// <param name="methodName">The name of the action method.</param>
        /// <param name="argumentTypes">The optional array of action method argument types.</param>
        /// <returns>A new or existing <see cref="ActionApiVersionConventionBuilder"/>.</returns>
        /// <remarks>The specified <paramref name="methodName">method name</paramref> must refer to a public, non-static action method.
        /// If there is only one corresponding match found, then the <paramref name="argumentTypes">argument types</paramref> are ignored;
        /// otherwise, the <paramref name="argumentTypes">argument types</paramref> are used for method overload resolution. Action
        /// methods that have the <see cref="NonActionAttribute"/> applied will also be ignored.</remarks>
        public static ActionApiVersionConventionBuilder Action( this IActionConventionBuilder builder, string methodName, params Type[] argumentTypes )
        {
            Arg.NotNull( builder, nameof( builder ) );

            var methods = builder.ControllerType.GetRuntimeMethods().Where( m => m.Name == methodName && IsAction( m ) ).ToArray();

            switch ( methods.Length )
            {
                case 0:
                    throw new MissingMethodException( SR.ActionMethodNotFound.FormatDefault( methodName ) );
                case 1:
                    return builder.Action( methods[0] );
            }

            argumentTypes = argumentTypes ?? Type.EmptyTypes;
            methods = methods.Where( m => SignatureMatches( m, argumentTypes ) ).ToArray();

            if ( methods.Length == 1 )
            {
                return builder.Action( methods[0] );
            }

            throw new AmbiguousMatchException( SR.AmbiguousActionMethod.FormatDefault( methodName ) );
        }

        static bool IsAction( MethodInfo method )
        {
            Contract.Requires( method != null );

            if ( !method.IsPublic || method.IsStatic )
            {
                return false;
            }

            return method.GetCustomAttribute<NonActionAttribute>() == null;
        }

        static bool SignatureMatches( MethodInfo method, Type[] argumentTypes )
        {
            Contract.Requires( method != null );
            Contract.Requires( argumentTypes != null );

            var argTypes = method.GetParameters().Select( p => p.ParameterType ).ToArray();
            return argTypes.SequenceEqual( argumentTypes );
        }
    }
}