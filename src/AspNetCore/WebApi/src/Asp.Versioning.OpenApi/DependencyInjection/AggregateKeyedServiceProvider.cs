// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace Microsoft.Extensions.DependencyInjection;

using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

internal sealed class AggregateKeyedServiceProvider : IKeyedServiceProvider, IDisposable
{
    private readonly IServiceCollection services;
    private readonly SemaphoreSlim semaphore = new SemaphoreSlim( 1, 1 );

    private IServiceProvider serviceProvider;
    private bool initialized;
    private int? initializingThreadId;

    public AggregateKeyedServiceProvider( IServiceProvider serviceProvider, IServiceCollection services )
    {
        this.services = services;
        this.serviceProvider = serviceProvider;
        var lifetime = serviceProvider.GetRequiredService<IHostApplicationLifetime>();
        lifetime.ApplicationStarted.Register( () => EnsureInitialized(true) );
    }

    private IServiceProvider ServiceProvider
    {
        get
        {
            EnsureInitialized(false);
            return serviceProvider;
        }
    }

    private void EnsureInitialized(bool isReady)
    {
        // If already initialized, we can return immediately.
        if ( initialized)
        {
            return;
        }

        if ( initializingThreadId.HasValue && Environment.CurrentManagedThreadId == initializingThreadId.Value )
        {
            return;
        }

        // If a "ready" call entered this call already, ensure that other calls will be blocked until we fully initialize.
        semaphore.Wait();
        try
        {
            if ( initialized || !isReady )
            {
                return;
            }

            initializingThreadId = Environment.CurrentManagedThreadId;
            var provider = serviceProvider.GetRequiredService<IApiVersionDescriptionProvider>();

            var collection = new ServiceCollection();
            foreach ( var descriptor in services )
            {
                collection.Add( descriptor );
            }

            var descriptions = provider.ApiVersionDescriptions;

            for ( var i = 0; i < descriptions.Count; i++ )
            {
                var description = descriptions[i];
                collection.AddOpenApi( description.GroupName );
            }

            serviceProvider = collection.BuildServiceProvider();
            initialized = true;
            initializingThreadId = null;
        }
        finally
        {
            semaphore.Release();
        }
    }

    public object? GetKeyedService( Type serviceType, object? serviceKey )
    {
        return ServiceProvider.GetKeyedService( serviceType, serviceKey );
    }

    public object GetRequiredKeyedService( Type serviceType, object? serviceKey )
    {
        return ServiceProvider.GetRequiredKeyedService( serviceType, serviceKey );
    }

    public object? GetService( Type serviceType )
        => ServiceProvider.GetService( serviceType );

    public void Dispose()
    {
        semaphore.Dispose();
    }
}