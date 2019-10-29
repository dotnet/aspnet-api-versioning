#pragma warning disable CA1812

namespace Microsoft.Web.Http.Routing
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Web.Http.Routing;
    using static System.Linq.Expressions.Expression;

    sealed class ParsedRouteAdapter<T> : IParsedRoute where T : notnull
    {
        static readonly Lazy<Func<T, IEnumerable<object>>> pathSegmentsAccessor = new Lazy<Func<T, IEnumerable<object>>>( NewPathSegmentsAccessor );
        static readonly Lazy<Func<T, IDictionary<string, object>?, IDictionary<string, object>, HttpRouteValueDictionary, HttpRouteValueDictionary, object>> bindFunc =
            new Lazy<Func<T, IDictionary<string, object>?, IDictionary<string, object>, HttpRouteValueDictionary, HttpRouteValueDictionary, object>>( NewBindFunc );
        readonly T adapted;
        readonly Lazy<IReadOnlyList<IPathSegment>> pathSegments;

        public ParsedRouteAdapter( T adapted )
        {
            this.adapted = adapted;
            pathSegments = new Lazy<IReadOnlyList<IPathSegment>>( AdaptToPathSegments );
        }

        public IBoundRouteTemplate Bind( IDictionary<string, object>? currentValues, IDictionary<string, object> values, HttpRouteValueDictionary defaultValues, HttpRouteValueDictionary constraints )
        {
            var boundRouteTemplate = bindFunc.Value( adapted, currentValues, values, defaultValues, constraints );
            var adapterType = typeof( BoundRouteTemplateAdapter<> ).MakeGenericType( boundRouteTemplate.GetType() );
            var adapter = (IBoundRouteTemplate) Activator.CreateInstance( adapterType, boundRouteTemplate );

            return adapter;
        }

        public IReadOnlyList<IPathSegment> PathSegments => pathSegments.Value;

        IReadOnlyList<IPathSegment> AdaptToPathSegments()
        {
            var pathSegments = pathSegmentsAccessor.Value( adapted );
            var adapters = new List<IPathSegment>();

            foreach ( var pathSegment in pathSegments )
            {
                var type = pathSegment.GetType();
                var adapter = default( IPathSegment );
                var adapterType = default( Type );

                switch ( type.Name )
                {
                    case "PathContentSegment":
                        adapterType = typeof( PathContentSegmentAdapter<> ).MakeGenericType( type );
                        adapter = (IPathSegment) Activator.CreateInstance( adapterType, pathSegment );
                        break;
                    case "PathSeparatorSegment":
                        adapterType = typeof( PathSeparatorSegmentAdapter<> ).MakeGenericType( type );
                        adapter = (IPathSegment) Activator.CreateInstance( adapterType, pathSegment );
                        break;
                    default:
                        throw new InvalidOperationException( $"Encountered the {type.Name} path segment, which was not expected." );
                }

                adapters.Add( adapter );
            }

            return adapters.ToArray();
        }

        static Func<T, IEnumerable<object>> NewPathSegmentsAccessor()
        {
            var o = Parameter( typeof( T ), "o" );
            var body = Property( o, nameof( PathSegments ) );
            var lambda = Lambda<Func<T, IEnumerable<object>>>( body, o );

            return lambda.Compile();
        }

        static Func<T, IDictionary<string, object>?, IDictionary<string, object>, HttpRouteValueDictionary, HttpRouteValueDictionary, object> NewBindFunc()
        {
            var o = Parameter( typeof( T ), "o" );
            var currentValues = Parameter( typeof( IDictionary<string, object> ), "currentValues" );
            var values = Parameter( typeof( IDictionary<string, object> ), "values" );
            var defaultValues = Parameter( typeof( HttpRouteValueDictionary ), "defaultValues" );
            var constraints = Parameter( typeof( HttpRouteValueDictionary ), "constraints" );
            var parameterTypes = new[] { typeof( IDictionary<string, object> ), typeof( IDictionary<string, object> ), typeof( HttpRouteValueDictionary ), typeof( HttpRouteValueDictionary ) };
            var method = typeof( T ).GetRuntimeMethod( nameof( Bind ), parameterTypes );
            var body = Call( o, method, currentValues, values, defaultValues, constraints );
            var lambda = Lambda<Func<T, IDictionary<string, object>?, IDictionary<string, object>, HttpRouteValueDictionary, HttpRouteValueDictionary, object>>( body, o, currentValues, values, defaultValues, constraints );

            return lambda.Compile();
        }
    }
}