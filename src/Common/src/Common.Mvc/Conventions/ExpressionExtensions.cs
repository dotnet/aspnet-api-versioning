// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

internal static class ExpressionExtensions
{
    extension<TDelegate>( Expression<TDelegate> expression )
    {
        internal MethodInfo ExtractMethod()
        {
            if ( expression.Body is MethodCallExpression methodCall )
            {
                return methodCall.Method;
            }

            var message = string.Format( CultureInfo.CurrentCulture, MvcFormat.InvalidActionMethodExpression, expression );
            throw new InvalidOperationException( message );
        }
    }
}