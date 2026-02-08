// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace System;

internal static class ServiceProviderExtensions
{
    extension( IServiceProvider serviceProvider )
    {
        internal IServiceProvider WithParent( IServiceProvider parent ) => new CompositeServiceProvider( serviceProvider, parent );

        internal TService WithParent<TService>( IServiceProvider parent, Func<IServiceProvider, TService> implementationFactory ) =>
            implementationFactory( serviceProvider.WithParent( parent ) );
    }

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