namespace Microsoft.Web.Http.Description
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Web.Http.Controllers;
    using System.Web.Http.Routing;
    using System.Web.OData.Routing;
    using static System.Linq.Enumerable;
    using static System.String;

    sealed class ODataRouteBuilder
    {
        readonly string routeTemplate;
        readonly IHttpRoute route;
        readonly HttpActionDescriptor actionDescriptor;

        internal ODataRouteBuilder( string routeTemplate, IHttpRoute route, HttpActionDescriptor actionDescriptor )
        {
            Contract.Requires( !IsNullOrEmpty( routeTemplate ) );
            Contract.Requires( route != null );
            Contract.Requires( actionDescriptor != null );

            this.routeTemplate = routeTemplate;
            this.route = route;
            this.actionDescriptor = actionDescriptor;
        }

        internal string Build()
        {
            if ( !( route is ODataRoute odataRoute ) )
            {
                return routeTemplate;
            }

            var segments = new List<string>();
            var prefix = odataRoute.RoutePrefix?.Trim( '/' );

            if ( !IsNullOrEmpty( prefix ) )
            {
                segments.Add( prefix );
            }

            var controllerDescriptor = actionDescriptor.ControllerDescriptor;
            var path = controllerDescriptor.GetCustomAttributes<ODataRoutePrefixAttribute>().FirstOrDefault()?.Prefix?.Trim( '/' );

            if ( IsNullOrEmpty( path ) )
            {
                path = controllerDescriptor.ControllerName;
            }

            var template = actionDescriptor.GetCustomAttributes<ODataRouteAttribute>().FirstOrDefault()?.PathTemplate;

            if ( !IsNullOrEmpty( template ) )
            {
                path += template;
            }

            segments.Add( path );

            return Join( "/", segments );
        }
    }
}