// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using static System.Linq.Expressions.Expression;

internal sealed class PathParameterSubsegmentAdapter<T> : IPathParameterSubsegment where T : notnull
{
    private static readonly Lazy<Func<T, bool>> catchAllAccessor = new( NewCatchAllAccessor );
    private static readonly Lazy<Func<T, string>> parameterNameAccessor = new( NewParameterNameAccessor );
    private readonly T adapted;

    public PathParameterSubsegmentAdapter( T adapted ) => this.adapted = adapted;

    public bool IsCatchAll => catchAllAccessor.Value( adapted );

    public string ParameterName => parameterNameAccessor.Value( adapted );

    public override string ToString() => adapted.ToString();

    private static Func<T, bool> NewCatchAllAccessor()
    {
        var o = Parameter( typeof( T ), "o" );
        var body = Property( o, nameof( IsCatchAll ) );
        var lambda = Lambda<Func<T, bool>>( body, o );

        return lambda.Compile();
    }

    private static Func<T, string> NewParameterNameAccessor()
    {
        var o = Parameter( typeof( T ), "o" );
        var body = Property( o, nameof( ParameterName ) );
        var lambda = Lambda<Func<T, string>>( body, o );

        return lambda.Compile();
    }
}