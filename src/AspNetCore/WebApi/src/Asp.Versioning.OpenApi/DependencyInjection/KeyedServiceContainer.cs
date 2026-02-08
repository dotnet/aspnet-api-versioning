// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace Microsoft.Extensions.DependencyInjection;

using System.ComponentModel.Design;

internal sealed class KeyedServiceContainer( IServiceProvider parent ) :
    IServiceProvider,
    IKeyedServiceProvider,
    IServiceProviderIsService,
    IServiceProviderIsKeyedService,
    IDisposable
{
    private readonly ServiceContainer services = new( parent );
    private readonly Dictionary<object, ServiceContainer> keyedServices = [];
    private bool disposed;

    public object? GetKeyedService( Type serviceType, object? serviceKey )
    {
        if ( serviceKey is not null && keyedServices.TryGetValue( serviceKey, out var container ) )
        {
            return container.GetService( serviceType );
        }

        return services.GetKeyedService( serviceType, serviceKey );
    }

    public object GetRequiredKeyedService( Type serviceType, object? serviceKey )
    {
        if ( serviceKey is not null && keyedServices.TryGetValue( serviceKey, out var container ) )
        {
            return container.GetRequiredService( serviceType );
        }

        return services.GetRequiredKeyedService( serviceType, serviceKey );
    }

    public object? GetService( Type serviceType ) => services.GetService( serviceType );

    public bool IsKeyedService( Type serviceType, object? serviceKey )
    {
        if ( serviceKey is not null && keyedServices.ContainsKey( serviceKey ) )
        {
            return true;
        }
        else if ( services.GetService<IServiceProviderIsKeyedService>() is { } service )
        {
            return service.IsKeyedService( serviceType, serviceKey );
        }

        return false;
    }

    public bool IsService( Type serviceType )
    {
        if ( services.GetService<IServiceProviderIsService>() is { } service
             && service.IsService( serviceType ) )
        {
            return true;
        }

        return services.GetService( serviceType ) is not null;
    }

    public void Add( Type serviceType, object instance ) => services.AddService( serviceType, instance );

    public void Add( Type serviceType, Func<IServiceProvider, object> activator ) =>
        services.AddService( serviceType, ( _, _ ) => activator( this ) );

    public void Add( Type serviceType, string serviceKey, Func<IServiceProvider, string, object> activator )
    {
        if ( !keyedServices.TryGetValue( serviceKey, out var container ) )
        {
            keyedServices.Add( serviceKey, container = new() );
        }

        container.AddService( serviceType, ( _, _ ) => activator( this, serviceKey ) );
    }

    public void Dispose()
    {
        if ( disposed )
        {
            return;
        }

        disposed = true;

        foreach ( var container in keyedServices.Values )
        {
            container.Dispose();
        }

        services.Dispose();
    }
}