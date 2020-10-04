namespace Microsoft.AspNetCore.Mvc.Abstractions
{
    using Microsoft.AspNetCore.Mvc.ActionConstraints;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.Routing;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using static System.Array;
    using static System.StringComparison;

    static class ActionDescriptorExtensions
    {
        const string DefaultHttpMethod = "POST";
        static readonly string[] SupportedHttpMethodConventions = new[]
        {
            "GET",
            "PUT",
            "POST",
            "DELETE",
            "PATCH",
            "HEAD",
            "OPTIONS",
        };

        internal static IEnumerable<string> GetHttpMethods( this ActionDescriptor action )
        {
            if ( action is ControllerActionDescriptor controllerAction )
            {
                return controllerAction.GetHttpMethods();
            }

            var constraints = ( action.ActionConstraints ?? Empty<IActionConstraintMetadata>() ).OfType<HttpMethodActionConstraint>();
            var definedHttpMethods = constraints.SelectMany( ac => ac.HttpMethods );
            var httpMethods = new HashSet<string>( definedHttpMethods, StringComparer.OrdinalIgnoreCase );

            if ( httpMethods.Count == 0 )
            {
                httpMethods.Add( DefaultHttpMethod );
            }

            return httpMethods;
        }

        internal static IEnumerable<string> GetHttpMethods( this ControllerActionDescriptor action )
        {
            var constraints = ( action.ActionConstraints ?? Empty<IActionConstraintMetadata>() ).OfType<HttpMethodActionConstraint>();
            var attributes = action.MethodInfo.GetCustomAttributes( inherit: false ).OfType<IActionHttpMethodProvider>();
            var definedHttpMethods = constraints.SelectMany( ac => ac.HttpMethods ).Concat( attributes.SelectMany( a => a.HttpMethods ) );
            var httpMethods = new HashSet<string>( definedHttpMethods, StringComparer.OrdinalIgnoreCase );

            if ( httpMethods.Count > 0 )
            {
                return httpMethods;
            }

            for ( var i = 0; i < SupportedHttpMethodConventions.Length; i++ )
            {
                var supportedHttpMethod = SupportedHttpMethodConventions[i];

                if ( action.MethodInfo.Name.StartsWith( supportedHttpMethod, OrdinalIgnoreCase ) )
                {
                    httpMethods.Add( supportedHttpMethod );
                }
            }

            if ( httpMethods.Count == 0 )
            {
                httpMethods.Add( DefaultHttpMethod );
            }

            return httpMethods;
        }
    }
}