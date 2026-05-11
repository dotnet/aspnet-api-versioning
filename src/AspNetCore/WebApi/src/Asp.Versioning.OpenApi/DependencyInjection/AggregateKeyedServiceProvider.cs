// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace Microsoft.Extensions.DependencyInjection;

internal sealed class AggregateKeyedServiceProvider( IServiceProvider parent ) : IKeyedServiceProvider
{
    private readonly IServiceProvider parent = parent;
    private readonly List<IServiceProvider> providers = [];

    public object? GetKeyedService( Type serviceType, object? serviceKey )
    {
        if ( providers.Count == 0 )
        {
            return parent.GetKeyedService( serviceType, serviceKey );
        }

        foreach ( var provider in providers )
        {
            if ( provider.GetKeyedService( serviceType, serviceKey ) is { } service )
            {
                return service;
            }
        }

        return null;
    }

    public object GetRequiredKeyedService( Type serviceType, object? serviceKey )
    {
        if ( providers.Count == 0 )
        {
            return parent.GetRequiredKeyedService( serviceType, serviceKey );
        }

        for ( int i = 0; i < providers.Count - 1; i++ )
        {
            if ( providers[i].GetKeyedService( serviceType, serviceKey ) is { } service )
            {
                return service;
            }
        }

        return providers[providers.Count - 1].GetRequiredKeyedService( serviceType, serviceKey );
    }

    public object? GetService( Type serviceType )
        => parent.GetService( serviceType );

    public void Add( IServiceCollection serviceCollection, IServiceCollection parentServiceCollection )
    {
        foreach ( var descriptor in parentServiceCollection )
        {
            serviceCollection.Add( descriptor );
        }

        providers.Add( serviceCollection.BuildServiceProvider() );
    }
}