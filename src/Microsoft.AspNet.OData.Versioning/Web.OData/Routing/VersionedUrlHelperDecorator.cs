namespace Microsoft.Web.OData.Routing
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Web.Http.Routing;

    internal sealed class VersionedUrlHelperDecorator : UrlHelper
    {
        private readonly UrlHelper decorated;
        private readonly object apiVersion;

        internal VersionedUrlHelperDecorator( UrlHelper decorated, object apiVersion )
        {
            Contract.Requires( decorated != null );
            Contract.Requires( apiVersion != null );

            this.decorated = decorated;
            this.apiVersion = apiVersion;

            if ( decorated.Request != null )
            {
                Request = decorated.Request;
            }
        }

        private void EnsureApiVersionRouteValue( IDictionary<string, object> routeValues ) => routeValues[nameof( apiVersion )] = apiVersion;

        public override string Content( string path ) => decorated.Content( path );

        public override string Link( string routeName, object routeValues ) => decorated.Link( routeName, routeValues );

        public override string Link( string routeName, IDictionary<string, object> routeValues )
        {
            EnsureApiVersionRouteValue( routeValues );
            return decorated.Link( routeName, routeValues );
        }

        public override string Route( string routeName, object routeValues ) => decorated.Route( routeName, routeValues );

        public override string Route( string routeName, IDictionary<string, object> routeValues )
        {
            EnsureApiVersionRouteValue( routeValues );
            return decorated.Route( routeName, routeValues );
        }
    }
}