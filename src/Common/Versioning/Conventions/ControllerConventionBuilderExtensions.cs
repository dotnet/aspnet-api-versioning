#if WEBAPI
namespace Microsoft.Web.Http.Versioning.Conventions
#else
namespace Microsoft.AspNetCore.Mvc.Versioning.Conventions
#endif
{
    using System;
    using System.Linq.Expressions;
#if WEBAPI
    using System.Web.Http;
    using System.Web.Http.Controllers;
#endif

    /// <summary>
    /// Provides extension methods for <see cref="IControllerConventionBuilder"/> and <see cref="IControllerConventionBuilder{T}"/> interfaces.
    /// </summary>
#if !WEBAPI
    [CLSCompliant( false )]
#endif
    public static class ControllerConventionBuilderExtensions
    {
        /// <summary>
        /// Gets or creates the convention builder for the specified controller action method.
        /// </summary>
        /// <typeparam name="TController">The type of controller.</typeparam>
        /// <param name="builder">The extended <see cref="IControllerConventionBuilder{T}"/>.</param>
        /// <param name="actionExpression">The <see cref="Expression{TDelegate}">expression</see> representing the controller action method.</param>
        /// <returns>A new or existing <see cref="ActionApiVersionConventionBuilder{T}"/>.</returns>
        public static IActionConventionBuilder<TController> Action<TController>( this IControllerConventionBuilder<TController> builder, Expression<Action<TController>> actionExpression )
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
        /// <param name="builder">The extended <see cref="ControllerApiVersionConventionBuilder{T}"/>.</param>
        /// <param name="actionExpression">The <see cref="Expression{TDelegate}">expression</see> representing the controller action method.</param>
        /// <returns>A new or existing <see cref="ActionApiVersionConventionBuilder{T}"/>.</returns>
        public static IActionConventionBuilder<TController> Action<TController, TResult>( this IControllerConventionBuilder<TController> builder, Expression<Func<TController, TResult>> actionExpression )
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
        public static IActionConventionBuilder Action( this IControllerConventionBuilder builder, string methodName, params Type[] argumentTypes )
        {
            Arg.NotNull( builder, nameof( builder ) );
            Arg.NotNullOrEmpty( methodName, nameof( methodName ) );

            var method = ActionMethodResolver.Resolve( builder.ControllerType, methodName, argumentTypes );
            return builder.Action( method );
        }
    }
}