#pragma warning disable CA1812

namespace Microsoft.Web.Http.Routing
{
    using System;
    using static System.Linq.Expressions.Expression;

    sealed class PathParameterSubsegmentAdapter<T> : IPathParameterSubsegment where T : notnull
    {
        static readonly Lazy<Func<T, bool>> catchAllAccessor = new Lazy<Func<T, bool>>( NewCatchAllAccessor );
        static readonly Lazy<Func<T, string>> parameterNameAccessor = new Lazy<Func<T, string>>( NewParameterNameAccessor );
        readonly T adapted;

        public PathParameterSubsegmentAdapter( T adapted ) => this.adapted = adapted;

        public bool IsCatchAll => catchAllAccessor.Value( adapted );

        public string ParameterName => parameterNameAccessor.Value( adapted );

        public override string ToString() => adapted.ToString();

        static Func<T, bool> NewCatchAllAccessor()
        {
            var o = Parameter( typeof( T ), "o" );
            var body = Property( o, nameof( IsCatchAll ) );
            var lambda = Lambda<Func<T, bool>>( body, o );

            return lambda.Compile();
        }

        static Func<T, string> NewParameterNameAccessor()
        {
            var o = Parameter( typeof( T ), "o" );
            var body = Property( o, nameof( ParameterName ) );
            var lambda = Lambda<Func<T, string>>( body, o );

            return lambda.Compile();
        }
    }
}