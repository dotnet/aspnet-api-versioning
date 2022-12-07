// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using System.Web.Http.Routing;
using static System.Linq.Expressions.Expression;

internal sealed class BoundRouteTemplateAdapter<T> : IBoundRouteTemplate where T : notnull
{
    private static readonly Lazy<Func<T, string>> boundTemplateAccessor = new( NewBoundTemplateAccessor );
    private static readonly Lazy<Action<T, string>> boundTemplateMutator = new( NewBoundTemplateMutator );
    private static readonly Lazy<Func<T, HttpRouteValueDictionary>> valuesAccessor = new( NewValuesAccessor );
    private static readonly Lazy<Action<T, HttpRouteValueDictionary>> valuesMutator = new( NewValuesMutator );
    private readonly T adapted;

    public BoundRouteTemplateAdapter( T adapted ) => this.adapted = adapted;

    public string BoundTemplate
    {
        get => boundTemplateAccessor.Value( adapted );
        set => boundTemplateMutator.Value( adapted, value );
    }

    public HttpRouteValueDictionary Values
    {
        get => valuesAccessor.Value( adapted );
        set => valuesMutator.Value( adapted, value );
    }

    private static Func<T, string> NewBoundTemplateAccessor()
    {
        var o = Parameter( typeof( T ), "o" );
        var body = Property( o, nameof( BoundTemplate ) );
        var lambda = Lambda<Func<T, string>>( body, o );

        return lambda.Compile();
    }

    private static Action<T, string> NewBoundTemplateMutator()
    {
        var o = Parameter( typeof( T ), "o" );
        var value = Parameter( typeof( string ), "value" );
        var body = Assign( Property( o, nameof( BoundTemplate ) ), value );
        var lambda = Lambda<Action<T, string>>( body, o, value );

        return lambda.Compile();
    }

    private static Func<T, HttpRouteValueDictionary> NewValuesAccessor()
    {
        var o = Parameter( typeof( T ), "o" );
        var body = Property( o, nameof( Values ) );
        var lambda = Lambda<Func<T, HttpRouteValueDictionary>>( body, o );

        return lambda.Compile();
    }

    private static Action<T, HttpRouteValueDictionary> NewValuesMutator()
    {
        var o = Parameter( typeof( T ), "o" );
        var value = Parameter( typeof( HttpRouteValueDictionary ), "value" );
        var body = Assign( Property( o, nameof( Values ) ), value );
        var lambda = Lambda<Action<T, HttpRouteValueDictionary>>( body, o, value );

        return lambda.Compile();
    }
}