namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using System;

    sealed class ApiVersionModelBinderProvider : IModelBinderProvider
    {
        static readonly Type ApiVersionType = typeof( ApiVersion );
        static readonly ApiVersionModelBinder binder = new ApiVersionModelBinder();

        public IModelBinder? GetBinder( ModelBinderProviderContext context )
        {
            if ( context == null )
            {
                throw new ArgumentNullException( nameof( context ) );
            }

            if ( ApiVersionType.IsAssignableFrom( context.Metadata.ModelType ) )
            {
                return binder;
            }

            return default;
        }
    }
}