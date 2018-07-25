﻿namespace Microsoft.AspNet.OData.Routing
{
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.ActionConstraints;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.Internal;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using static System.StringComparison;

    sealed class ImplicitHttpMethodConvention : IODataActionDescriptorConvention
    {
        static readonly IReadOnlyList<string> SupportedHttpMethodConventions = new[]
        {
            "GET",
            "PUT",
            "POST",
            "DELETE",
            "PATCH",
            "HEAD",
            "OPTIONS",
        };

        public void Apply( ActionDescriptorProviderContext context, ControllerActionDescriptor action )
        {
            Contract.Requires( context != null );
            Contract.Requires( action != null );

            var odataActionWithExplicitHttpMethods =
                action.ActionConstraints != null &&
                action.ActionConstraints.OfType<HttpMethodActionConstraint>().Any();

            if ( odataActionWithExplicitHttpMethods )
            {
                return;
            }

            var httpMethod = GetImplicitHttpMethod( action );
            var actionConstraints = action.ActionConstraints ?? new List<IActionConstraintMetadata>();

            actionConstraints.Add( new HttpMethodActionConstraint( new[] { httpMethod } ) );
            action.ActionConstraints = actionConstraints;
        }

        static string GetImplicitHttpMethod( ControllerActionDescriptor action )
        {
            Contract.Requires( action != null );
            Contract.Ensures( !string.IsNullOrEmpty( Contract.Result<string>() ) );

            const int Post = 2;

            foreach ( var supportedHttpMethod in SupportedHttpMethodConventions )
            {
                if ( action.MethodInfo.Name.StartsWith( supportedHttpMethod, OrdinalIgnoreCase ) )
                {
                    return supportedHttpMethod;
                }
            }

            return SupportedHttpMethodConventions[Post];
        }
    }
}