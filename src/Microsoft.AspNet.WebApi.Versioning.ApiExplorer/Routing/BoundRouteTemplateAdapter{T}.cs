#pragma warning disable CA1812

namespace Microsoft.Web.Http.Routing
{
    using System;
    using System.Web.Http.Routing;
    using static System.Linq.Expressions.Expression;

    sealed class BoundRouteTemplateAdapter<T> : IBoundRouteTemplate where T : notnull
    {
        static readonly Lazy<Func<T, string>> boundTemplateAccessor = new Lazy<Func<T, string>>( NewBoundTemplateAccessor );
        static readonly Lazy<Action<T, string>> boundTemplateMutator = new Lazy<Action<T, string>>( NewBoundTemplateMutator );
        static readonly Lazy<Func<T, HttpRouteValueDictionary>> valuesAccessor = new Lazy<Func<T, HttpRouteValueDictionary>>( NewValuesAccessor );
        static readonly Lazy<Action<T, HttpRouteValueDictionary>> valuesMutator = new Lazy<Action<T, HttpRouteValueDictionary>>( NewValuesMutator );
        readonly T adapted;

        public BoundRouteTemplateAdapter( T adapted ) => this.adapted = adapted;

        public string BoundTemplate
        {
            get => boundTemplateAccessor.Value( adapted );
            set => boundTemplateMutator.Value( adapted, value );
        }

#pragma warning disable CA2227 // Collection properties should be read only
        public HttpRouteValueDictionary Values
#pragma warning restore CA2227
        {
            get => valuesAccessor.Value( adapted );
            set => valuesMutator.Value( adapted, value );
        }

        static Func<T, string> NewBoundTemplateAccessor()
        {
            var o = Parameter( typeof( T ), "o" );
            var body = Property( o, nameof( BoundTemplate ) );
            var lambda = Lambda<Func<T, string>>( body, o );

            return lambda.Compile();
        }

        static Action<T, string> NewBoundTemplateMutator()
        {
            var o = Parameter( typeof( T ), "o" );
            var value = Parameter( typeof( string ), "value" );
            var body = Assign( Property( o, nameof( BoundTemplate ) ), value );
            var lambda = Lambda<Action<T, string>>( body, o, value );

            return lambda.Compile();
        }

        static Func<T, HttpRouteValueDictionary> NewValuesAccessor()
        {
            var o = Parameter( typeof( T ), "o" );
            var body = Property( o, nameof( Values ) );
            var lambda = Lambda<Func<T, HttpRouteValueDictionary>>( body, o );

            return lambda.Compile();
        }

        static Action<T, HttpRouteValueDictionary> NewValuesMutator()
        {
            var o = Parameter( typeof( T ), "o" );
            var value = Parameter( typeof( HttpRouteValueDictionary ), "value" );
            var body = Assign( Property( o, nameof( Values ) ), value );
            var lambda = Lambda<Action<T, HttpRouteValueDictionary>>( body, o, value );

            return lambda.Compile();
        }
    }
}