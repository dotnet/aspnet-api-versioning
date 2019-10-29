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
    using System.Collections.Generic;
    using System.Linq;

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

        public int Order => 0;

        public void OnProvidersExecuted( ActionDescriptorProviderContext context )
        {
            if ( routeCollectionProvider.Items.Count == 0 )
            {
                return;
            }

            var results = context.Results.ToArray();
            var conventions = new IODataActionDescriptorConvention[]
            {
                new ImplicitHttpMethodConvention(),
                new ODataRouteBindingInfoConvention( routeCollectionProvider, modelMetadataProvider, options ),
            };

            foreach ( var action in ODataActions( results ) )
            {
                foreach ( var convention in conventions )
                {
                    convention.Apply( context, action );
                }
            }
        }

        public void OnProvidersExecuting( ActionDescriptorProviderContext context ) { }

        static IEnumerable<ControllerActionDescriptor> ODataActions( IEnumerable<ActionDescriptor> results )
        {
            foreach ( var result in results )
            {
                if ( result is ControllerActionDescriptor action &&
                     action.ControllerTypeInfo.IsODataController() &&
                    !action.ControllerTypeInfo.IsMetadataController() )
                {
                    yield return action;
                }
            }
        }
    }
}