#if WEBAPI
namespace Microsoft.Web.Http.Versioning.Conventions
#else
namespace Microsoft.AspNetCore.Mvc.Versioning.Conventions
#endif
{
    using System;
    using System.Linq.Expressions;
#if WEBAPI
    using System.Web.Http.Controllers;
#endif

    /// <summary>
    /// Provides extension methods for <see cref="IActionConventionBuilder{T}"/>.
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
        /// <param name="actionExpression">The <see cref="Expression{TDelegate}">expression</see> representing the controller action method</param>
        /// <returns>A new or existing <see cref="ActionApiVersionConventionBuilder{T}"/>.</returns>
        public static ActionApiVersionConventionBuilder<TController> Action<TController>( this IActionConventionBuilder<TController> builder, Expression<Action<TController>> actionExpression )
#if WEBAPI
            where TController : IHttpController
#endif
        {
            Arg.NotNull( builder, nameof( builder ) );
            return builder.Action( actionExpression.ExtractMethod() );
        }

        /// <summary>
        /// Gets or creates the convention builder for the specified controller action method.
        /// </summary>
        /// <typeparam name="TController">The type of controller.</typeparam>
        /// <typeparam name="TResult">The type of action result.</typeparam>
        /// <param name="builder">The extended <see cref="IActionConventionBuilder{T}"/>.</param>
        /// <param name="actionExpression">The <see cref="Expression{TDelegate}">expression</see> representing the controller action method</param>
        /// <returns>A new or existing <see cref="ActionApiVersionConventionBuilder{T}"/>.</returns>
        public static ActionApiVersionConventionBuilder<TController> Action<TController, TResult>( this IActionConventionBuilder<TController> builder, Expression<Func<TController, TResult>> actionExpression )
#if WEBAPI
            where TController : IHttpController
#endif
        {
            Arg.NotNull( builder, nameof( builder ) );
            return builder.Action( actionExpression.ExtractMethod() );
        }
    }
}