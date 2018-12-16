#if WEBAPI
namespace Microsoft.Web
#else
namespace Microsoft.AspNetCore.Mvc
#endif
{
    using System;
    using System.Collections.Generic;
#if !WEBAPI
    using Type = System.Reflection.TypeInfo;
#endif

    public abstract partial class HttpServerFixture : IDisposable
    {
        bool disposed;

        ~HttpServerFixture() => Dispose( false );

        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        public ICollection<Type> FilteredControllerTypes { get; } = new FilteredControllerTypes();
    }
}
