// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0079
#pragma warning disable IDE0130

namespace System.Web.Http.Controllers;

using System.Net.Http;
using System.Web.Http.Routing;

internal static class HttpActionDescriptorExtensions
{
    extension( HttpActionDescriptor actionDescriptor )
    {
        internal IList<HttpMethod> GetHttpMethods( IHttpRoute route )
        {
            IList<HttpMethod> actionHttpMethods = actionDescriptor.SupportedHttpMethods;
            var httpMethodConstraint = route.Constraints.Values.OfType<HttpMethodConstraint>().FirstOrDefault();

            if ( httpMethodConstraint == null )
            {
                return actionHttpMethods;
            }

            return [.. httpMethodConstraint.AllowedMethods.Intersect( actionHttpMethods )];
        }
    }
}