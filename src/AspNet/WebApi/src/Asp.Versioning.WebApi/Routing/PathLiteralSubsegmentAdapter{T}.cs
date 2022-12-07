// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using static System.Linq.Expressions.Expression;

internal sealed class PathLiteralSubsegmentAdapter<T> : IPathLiteralSubsegment where T : notnull
{
    private static readonly Lazy<Func<T, string>> literalAccessor = new( NewLiteralAccessor );
    private readonly T adapted;

    public PathLiteralSubsegmentAdapter( T adapted ) => this.adapted = adapted;

    public string Literal => literalAccessor.Value( adapted );

    public override string ToString() => adapted.ToString();

    private static Func<T, string> NewLiteralAccessor()
    {
        var o = Parameter( typeof( T ), "o" );
        var body = Property( o, nameof( Literal ) );
        var lambda = Lambda<Func<T, string>>( body, o );

        return lambda.Compile();
    }
}