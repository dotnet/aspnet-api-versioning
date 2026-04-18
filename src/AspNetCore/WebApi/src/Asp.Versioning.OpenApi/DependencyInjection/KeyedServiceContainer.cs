// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace Microsoft.Extensions.DependencyInjection;

using System.ComponentModel.Design;

internal sealed class KeyedServiceContainer( IServiceProvider parent ) : ServiceContainer( parent ), IKeyedServiceProvider
{
    private readonly IServiceProvider parent = parent;
    private readonly Dictionary<object, ServiceContainer> keyedServices = [];
    private bool disposed;

    private object? GetKeyedService( Type serviceType, object? serviceKey )
    {
        if ( serviceKey is not null && keyedServices.TryGetValue( serviceKey, out var container ) )
        {
            if ( container.GetService( serviceType ) is { } service )
            {
                return service;
            }
        }

        return default;
    }

    object? IKeyedServiceProvider.GetKeyedService( Type serviceType, object? serviceKey ) =>
        GetKeyedService( serviceType, serviceKey ) ?? parent.GetKeyedService( serviceType, serviceKey );

    object IKeyedServiceProvider.GetRequiredKeyedService( Type serviceType, object? serviceKey ) =>
        GetKeyedService( serviceType, serviceKey ) ?? parent.GetRequiredKeyedService( serviceType, serviceKey );

    public void AddService( Type serviceType, Func<IServiceProvider, object> activator ) =>
        AddService( serviceType, ( sp, _ ) => activator( sp ) );

    public void AddService( Type serviceType, string serviceKey, Func<IServiceProvider, string, object> activator )
    {
        if ( !keyedServices.TryGetValue( serviceKey, out var container ) )
        {
            keyedServices.Add( serviceKey, container = new() );
        }

        container.AddService( serviceType, ( _, _ ) => activator( this, serviceKey ) );
    }

    protected override void Dispose( bool disposing )
    {
        base.Dispose( disposing );

        if ( disposed )
        {
            return;
        }

        disposed = true;

        foreach ( var container in keyedServices.Values )
        {
            container.Dispose();
        }
    }
}