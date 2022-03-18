// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using Microsoft.AspNetCore.Mvc.ModelBinding;

internal sealed class ApiVersionModelBinderProvider : IModelBinderProvider
{
    private static ApiVersionModelBinder? binder;

    public IModelBinder? GetBinder( ModelBinderProviderContext context )
    {
        if ( context == null )
        {
            throw new ArgumentNullException( nameof( context ) );
        }

        if ( typeof( ApiVersion ).IsAssignableFrom( context.Metadata.ModelType ) )
        {
            binder ??= new();
            return binder;
        }

        return default;
    }
}