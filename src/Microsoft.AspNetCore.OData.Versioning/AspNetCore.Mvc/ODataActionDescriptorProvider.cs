#pragma warning disable CA1812

namespace Microsoft.AspNetCore.Mvc
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Routing;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.Extensions.Options;
    using static Microsoft.AspNetCore.Mvc.Versioning.ODataApplicationModelProvider;

    sealed class ODataActionDescriptorProvider : IActionDescriptorProvider
    {
        readonly IODataRouteCollectionProvider routeCollectionProvider;
        readonly IModelMetadataProvider modelMetadataProvider;
        readonly IOptions<ODataApiVersioningOptions> options;

        public ODataActionDescriptorProvider(
            IODataRouteCollectionProvider routeCollectionProvider,
            IModelMetadataProvider modelMetadataProvider,
            IOptions<ODataApiVersioningOptions> options )
        {
            this.routeCollectionProvider = routeCollectionProvider;
            this.modelMetadataProvider = modelMetadataProvider;
            this.options = options;
        }

        public int Order => AfterApiVersioningConventions;

        public void OnProvidersExecuted( ActionDescriptorProviderContext context )
        {
            if ( routeCollectionProvider.Items.Count == 0 )
            {
                return;
            }

            var results = context.Results;
            var conventions = new IODataActionDescriptorConvention[]
            {
                new ImplicitHttpMethodConvention(),
                new ODataRouteBindingInfoConvention( routeCollectionProvider, modelMetadataProvider, options ),
            };

            for ( var i = results.Count - 1; i >= 0; i-- )
            {
                if ( results[i] is not ControllerActionDescriptor action ||
                     !action.ControllerTypeInfo.IsODataController() )
                {
                    continue;
                }

                results.RemoveAt( i );

                for ( var j = 0; j < conventions.Length; j++ )
                {
                    conventions[j].Apply( context, action );
                }
            }
        }

        public void OnProvidersExecuting( ActionDescriptorProviderContext context ) { }
    }
}