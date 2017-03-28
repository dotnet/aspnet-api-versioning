namespace Microsoft.AspNetCore.Mvc.Versioning.ApiExplorer
{
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.ApiExplorer;
    using System.Collections.Generic;
    using System.Linq;

    static class IApiDescriptionProviderExtensions
    {
        internal static IReadOnlyList<ApiDescription> Execute( this IApiDescriptionProvider apiDescriptionProvider, ActionDescriptor actionDescriptor )
        {
            var context = new ApiDescriptionProviderContext( new[] { actionDescriptor } );

            apiDescriptionProvider.OnProvidersExecuting( context );
            apiDescriptionProvider.OnProvidersExecuted( context );

            return context.Results.ToArray();
        }
    }
}