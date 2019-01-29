namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using System;
    using System.Diagnostics.Contracts;

    sealed class ApiVersionModelBinderProvider : IModelBinderProvider
    {
        static readonly Type ApiVersionType = typeof( ApiVersion );
        static readonly ApiVersionModelBinder binder = new ApiVersionModelBinder();

        public IModelBinder GetBinder( ModelBinderProviderContext context )
        {
            Contract.Assert( context != null );

            if ( ApiVersionType.IsAssignableFrom( context.Metadata.ModelType ) )
            {
                return binder;
            }

            return default;
        }
    }
}