#pragma warning disable CA1812

namespace Microsoft.Web.Http.Routing
{
    using System;
    using static System.Linq.Expressions.Expression;

    sealed class PathLiteralSubsegmentAdapter<T> : IPathLiteralSubsegment where T : notnull
    {
        static readonly Lazy<Func<T, string>> literalAccessor = new Lazy<Func<T, string>>( NewLiteralAccessor );
        readonly T adapted;

        public PathLiteralSubsegmentAdapter( T adapted ) => this.adapted = adapted;

        public string Literal => literalAccessor.Value( adapted );

        public override string ToString() => adapted.ToString();

        static Func<T, string> NewLiteralAccessor()
        {
            var o = Parameter( typeof( T ), "o" );
            var body = Property( o, nameof( Literal ) );
            var lambda = Lambda<Func<T, string>>( body, o );

            return lambda.Compile();
        }
    }
}