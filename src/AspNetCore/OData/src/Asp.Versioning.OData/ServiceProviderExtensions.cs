// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace System;

internal static class ServiceProviderExtensions
{
    internal static IServiceProvider WithParent( this IServiceProvider serviceProvider, IServiceProvider parent ) =>
        new CompositeServiceProvider( serviceProvider, parent );

    internal static TService WithParent<TService>(
        this IServiceProvider serviceProvider,
        IServiceProvider parent,
        Func<IServiceProvider, TService> implementationFactory ) =>
        implementationFactory( serviceProvider.WithParent( parent ) );

    private sealed class CompositeServiceProvider : IServiceProvider
    {
        private readonly IServiceProvider parent;
        private readonly IServiceProvider child;

        internal CompositeServiceProvider( IServiceProvider child, IServiceProvider parent )
        {
            this.parent = parent;
            this.child = child;
        }

        public object? GetService( Type serviceType ) =>
            child.GetService( serviceType ) ?? parent.GetService( serviceType );
    }
}