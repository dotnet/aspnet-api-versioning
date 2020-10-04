namespace System.Web.Http.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Web.Http.Routing;

    static class HttpActionDescriptorExtensions
    {
        internal static IList<HttpMethod> GetHttpMethods( this HttpActionDescriptor actionDescriptor, IHttpRoute route )
        {
            IList<HttpMethod> actionHttpMethods = actionDescriptor.SupportedHttpMethods;
            var httpMethodConstraint = route.Constraints.Values.OfType<HttpMethodConstraint>().FirstOrDefault();

            if ( httpMethodConstraint == null )
            {
                return actionHttpMethods;
            }

            return httpMethodConstraint.AllowedMethods.Intersect( actionHttpMethods ).ToList();
        }
    }
}