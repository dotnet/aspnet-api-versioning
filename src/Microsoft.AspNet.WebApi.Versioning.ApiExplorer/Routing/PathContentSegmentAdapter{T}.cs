#pragma warning disable CA1812

namespace Microsoft.Web.Http.Routing
{
    using System;
    using System.Collections.Generic;
    using static System.Linq.Expressions.Expression;

    sealed class PathContentSegmentAdapter<T> : IPathContentSegment where T : notnull
    {
        static readonly Lazy<Func<T, bool>> catchAllAccessor = new Lazy<Func<T, bool>>( NewCatchAllAccessor );
        static readonly Lazy<Func<T, IEnumerable<object>>> subsegmentsAccessor = new Lazy<Func<T, IEnumerable<object>>>( NewSubsegmentsAccessor );
        readonly T adapted;
        readonly Lazy<IReadOnlyList<IPathSubsegment>> subsegments;

        public PathContentSegmentAdapter( T adapted )
        {
            this.adapted = adapted;
            subsegments = new Lazy<IReadOnlyList<IPathSubsegment>>( AdaptToPathSubsegments );
        }

        public bool IsCatchAll => catchAllAccessor.Value( adapted );

        public IReadOnlyList<IPathSubsegment> Subsegments => subsegments.Value;

        public override string ToString() => adapted.ToString();

        IReadOnlyList<IPathSubsegment> AdaptToPathSubsegments()
        {
            var subsegments = subsegmentsAccessor.Value( adapted );
            var adapters = new List<IPathSubsegment>();

            foreach ( var subsegment in subsegments )
            {
                var type = subsegment.GetType();
                var adapter = default( IPathSubsegment );

                switch ( type.Name )
                {
                    case "PathLiteralSubsegment":
                        {
                            var adapterType = typeof( PathLiteralSubsegmentAdapter<> ).MakeGenericType( type );
                            adapter = (IPathSubsegment) Activator.CreateInstance( adapterType, subsegment );
                            break;
                        }

                    case "PathParameterSubsegment":
                        {
                            var adapterType = typeof( PathParameterSubsegmentAdapter<> ).MakeGenericType( type );
                            adapter = (IPathSubsegment) Activator.CreateInstance( adapterType, subsegment );
                            break;
                        }

                    default:
                        throw new InvalidOperationException( $"Encountered the {type.Name} path subsegment, which was not expected." );
                }

                adapters.Add( adapter );
            }

            return adapters.ToArray();
        }

        static Func<T, bool> NewCatchAllAccessor()
        {
            var o = Parameter( typeof( T ), "o" );
            var body = Property( o, nameof( IsCatchAll ) );
            var lambda = Lambda<Func<T, bool>>( body, o );

            return lambda.Compile();
        }

        static Func<T, IEnumerable<object>> NewSubsegmentsAccessor()
        {
            var o = Parameter( typeof( T ), "o" );
            var body = Property( o, nameof( Subsegments ) );
            var lambda = Lambda<Func<T, IEnumerable<object>>>( body, o );

            return lambda.Compile();
        }
    }
}