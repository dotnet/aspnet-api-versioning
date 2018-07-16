namespace Microsoft.AspNetCore.Mvc
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.ActionConstraints;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.Internal;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using static System.StringComparison;

    sealed class ODataSupportedHttpMethodProvider : IActionDescriptorProvider
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

        public int Order => 0;

        public void OnProvidersExecuted( ActionDescriptorProviderContext context )
        {
            foreach ( var result in context.Results )
            {
                if ( !( result is ControllerActionDescriptor action ) )
                {
                    continue;
                }

                var odataActionWithExplicitHttpMethods =
                    action.ControllerTypeInfo.IsODataController() &&
                    action.ActionConstraints != null &&
                    action.ActionConstraints.OfType<HttpMethodActionConstraint>().Any();

                if ( odataActionWithExplicitHttpMethods )
                {
                    continue;
                }

                var httpMethod = GetImplicitHttpMethod( action );
                var actionConstraints = action.ActionConstraints ?? new List<IActionConstraintMetadata>();

                actionConstraints.Add( new HttpMethodActionConstraint( new[] { httpMethod } ) );
                action.ActionConstraints = actionConstraints;
            }
        }

        public void OnProvidersExecuting( ActionDescriptorProviderContext context ) { }

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