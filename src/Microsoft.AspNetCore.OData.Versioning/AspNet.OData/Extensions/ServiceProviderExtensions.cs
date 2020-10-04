namespace Microsoft.AspNet.OData.Extensions
{
    using System;

    static class ServiceProviderExtensions
    {
        internal static IServiceProvider WithParent( this IServiceProvider serviceProvider, IServiceProvider parent ) =>
            new ServiceProviderAggregator( serviceProvider, parent );

        internal static TService WithParent<TService>(
            this IServiceProvider serviceProvider,
            IServiceProvider parent,
            Func<IServiceProvider, TService> implementationFactory ) => implementationFactory( serviceProvider.WithParent( parent ) );

        sealed class ServiceProviderAggregator : IServiceProvider
        {
            readonly IServiceProvider parent;
            readonly IServiceProvider child;

            internal ServiceProviderAggregator( IServiceProvider child, IServiceProvider parent )
            {
                this.parent = parent;
                this.child = child;
            }

            public object GetService( Type serviceType ) => child.GetService( serviceType ) ?? parent.GetService( serviceType );
        }
    }
}