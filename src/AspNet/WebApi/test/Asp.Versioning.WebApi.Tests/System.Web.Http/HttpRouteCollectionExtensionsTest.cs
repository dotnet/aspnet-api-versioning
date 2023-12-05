// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable SA1402 // File may only contain a single type
#pragma warning disable SA1403 // File may only contain a single namespace

namespace System.Web.Http
{
    using System.Web.Http.Routing;
    using System.Web.Http.WebHost.Routing;

    public class HttpRouteCollectionExtensionsTest
    {
        [Fact]
        public void to_dictionary_should_convert_route_collection()
        {
            // arrange
            var route = Mock.Of<IHttpRoute>();
            var routes = new HttpRouteCollection()
            {
                { "test", route },
            };

            // act
            var dictionary = routes.ToDictionary();

            // assert
            dictionary.Should().BeEquivalentTo( new Dictionary<string, IHttpRoute>() { ["test"] = route } );
        }

        [Fact]
        public void to_dictionary_should_convert_route_collection_when_hosted_with_SystemX2EWeb()
        {
            // arrange
            var route = Mock.Of<IHttpRoute>();
            var routes = new HostedHttpRouteCollection() { { "test", route } };

            // act
            var dictionary = routes.ToDictionary();

            // assert
            dictionary.Should().BeEquivalentTo( new Dictionary<string, IHttpRoute>() { ["test"] = route } );
        }
    }
}

// note: HostedHttpRouteCollection is an internal type. in order to test the expected behavior of the
// HttpRouteCollectionExtensions.ToDictionary hack, the bare minimum implementation is duplicated here
namespace System.Web.Http.WebHost.Routing
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Web.Http.Routing;
    using System.Web.Routing;

    internal sealed class HostedHttpRouteCollection : HttpRouteCollection
    {
#pragma warning disable SA1309 // Field names should not begin with underscore
        private readonly RouteCollection _routeCollection = [];
#pragma warning restore SA1309 // Field names should not begin with underscore

        public override string VirtualPathRoot => throw NotUsedInUnitTest();

        public override int Count => _routeCollection.Count;

        public override IHttpRoute this[string name] => ( (HttpWebRoute) _routeCollection[name] ).HttpRoute;

        public override IHttpRoute this[int index] => ( (HttpWebRoute) _routeCollection[index] ).HttpRoute;

        public override IHttpRouteData GetRouteData( HttpRequestMessage request ) => throw NotUsedInUnitTest();

        public override IHttpVirtualPathData GetVirtualPath( HttpRequestMessage request, string name, IDictionary<string, object> values ) => throw NotUsedInUnitTest();

        public override IHttpRoute CreateRoute( string uriTemplate, IDictionary<string, object> defaults, IDictionary<string, object> constraints, IDictionary<string, object> dataTokens, HttpMessageHandler handler ) => throw NotUsedInUnitTest();

        public override void Add( string name, IHttpRoute route ) => _routeCollection.Add( name, new HttpWebRoute( route ) );

        public override void Clear() => _routeCollection.Clear();

        public override bool Contains( IHttpRoute item )
        {
            foreach ( var route in _routeCollection )
            {
                if ( route is HttpWebRoute webRoute && webRoute.HttpRoute == item )
                {
                    return true;
                }
            }

            return false;
        }

        public override bool ContainsKey( string name ) => _routeCollection[name] != null;

        public override void CopyTo( IHttpRoute[] array, int arrayIndex ) => throw NotSupportedByHostedRouteCollection();

        public override void CopyTo( KeyValuePair<string, IHttpRoute>[] array, int arrayIndex ) => throw NotSupportedByRouteCollection();

        public override void Insert( int index, string name, IHttpRoute value ) => throw NotSupportedByRouteCollection();

        public override bool Remove( string name ) => throw NotSupportedByRouteCollection();

        public override IEnumerator<IHttpRoute> GetEnumerator() => _routeCollection.OfType<HttpWebRoute>().Select( r => r.HttpRoute ).GetEnumerator();

        public override bool TryGetValue( string name, out IHttpRoute route )
        {
            if ( _routeCollection[name] is HttpWebRoute rt )
            {
                route = rt.HttpRoute;
                return true;
            }

            route = null;
            return false;
        }

        private static NotSupportedException NotSupportedByRouteCollection() => new();

        private static NotSupportedException NotSupportedByHostedRouteCollection() => new();

        private static NotImplementedException NotUsedInUnitTest() => new( "Not used in unit tests" );
    }
}

namespace System.Web.Http.WebHost.Routing
{
    using Moq;
    using System.Web.Http.Routing;
    using System.Web.Routing;

    internal sealed class HttpWebRoute : Route
    {
        public HttpWebRoute( IHttpRoute httpRoute )
            : base( httpRoute.RouteTemplate, [], [], [], Mock.Of<IRouteHandler>() ) => HttpRoute = httpRoute;

        public IHttpRoute HttpRoute { get; }
    }
}