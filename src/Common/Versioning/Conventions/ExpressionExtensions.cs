#if WEBAPI
namespace Microsoft.Web.Http.Versioning.Conventions
#else
namespace Microsoft.AspNetCore.Mvc.Versioning.Conventions
#endif
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Linq.Expressions;
    using System.Reflection;

    internal static class ExpressionExtensions
    {
        internal static MethodInfo ExtractMethod<TDelegate>( this Expression<TDelegate> expression )
        {
            Contract.Requires( expression != null );
            Contract.Ensures( Contract.Result<MethodInfo>() != null );

            var methodCall = expression.Body as MethodCallExpression;

            if ( methodCall == null )
            {
                throw new InvalidOperationException( SR.InvalidActionMethodExpression.FormatDefault( expression ) );
            }

            return methodCall.Method;
        }
    }
}