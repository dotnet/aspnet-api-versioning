// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApiExplorer;

using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

internal static class IApiDescriptionProviderExtensions
{
    extension( IApiDescriptionProvider apiDescriptionProvider )
    {
        internal IReadOnlyList<ApiDescription> Execute( ActionDescriptor actionDescriptor )
        {
            var context = new ApiDescriptionProviderContext( [actionDescriptor] );

            apiDescriptionProvider.OnProvidersExecuting( context );
            apiDescriptionProvider.OnProvidersExecuted( context );

            return [.. context.Results];
        }
    }
}