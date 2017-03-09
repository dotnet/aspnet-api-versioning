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

    static class ExpressionExtensions
    {
        internal static MethodInfo ExtractMethod<TDelegate>( this Expression<TDelegate> expression )
        {
            Contract.Requires( expression != null );
            Contract.Ensures( Contract.Result<MethodInfo>() != null );

            if ( expression.Body is MethodCallExpression methodCall )
            {
                return methodCall.Method;
            }

            throw new InvalidOperationException( SR.InvalidActionMethodExpression.FormatDefault( expression ) );
        }
    }
}