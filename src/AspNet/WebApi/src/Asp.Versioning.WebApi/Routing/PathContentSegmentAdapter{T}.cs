// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using static System.Linq.Expressions.Expression;

internal sealed class PathContentSegmentAdapter<T> : IPathContentSegment where T : notnull
{
    private static readonly Lazy<Func<T, bool>> catchAllAccessor = new( NewCatchAllAccessor );
    private static readonly Lazy<Func<T, IEnumerable<object>>> subsegmentsAccessor = new( NewSubsegmentsAccessor );
    private readonly T adapted;
    private readonly Lazy<IReadOnlyList<IPathSubsegment>> subsegmentsHolder;

    public PathContentSegmentAdapter( T adapted )
    {
        this.adapted = adapted;
        subsegmentsHolder = new Lazy<IReadOnlyList<IPathSubsegment>>( AdaptToPathSubsegments );
    }

    public bool IsCatchAll => catchAllAccessor.Value( adapted );

    public IReadOnlyList<IPathSubsegment> Subsegments => subsegmentsHolder.Value;

    public override string ToString() => adapted.ToString();

    private IReadOnlyList<IPathSubsegment> AdaptToPathSubsegments()
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

    private static Func<T, bool> NewCatchAllAccessor()
    {
        var o = Parameter( typeof( T ), "o" );
        var body = Property( o, nameof( IsCatchAll ) );
        var lambda = Lambda<Func<T, bool>>( body, o );

        return lambda.Compile();
    }

    private static Func<T, IEnumerable<object>> NewSubsegmentsAccessor()
    {
        var o = Parameter( typeof( T ), "o" );
        var body = Property( o, nameof( Subsegments ) );
        var lambda = Lambda<Func<T, IEnumerable<object>>>( body, o );

        return lambda.Compile();
    }
}