namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Abstractions;
    using AspNetCore.Routing;
    using Extensions.Logging;
    using Extensions.Options;
    using Infrastructure;
    using Internal;
    using Microsoft.AspNetCore.Mvc.Routing;
    using System.Collections.Generic;

    public class TestODataApiVersionActionSelector : ODataApiVersionActionSelector
    {
        public TestODataApiVersionActionSelector(
            IActionDescriptorCollectionProvider actionDescriptorCollectionProvider,
            ActionConstraintCache actionConstraintCache,
            IOptions<ApiVersioningOptions> options,
            ILoggerFactory loggerFactory,
            IApiVersionRoutePolicy routePolicy )
            : base( actionDescriptorCollectionProvider, actionConstraintCache, options, loggerFactory, routePolicy ) { }

        public override ActionDescriptor SelectBestCandidate( RouteContext context, IReadOnlyList<ActionDescriptor> candidates ) =>
            SelectedCandidate = base.SelectBestCandidate( context, candidates );

        public ActionDescriptor SelectedCandidate { get; private set; }
    }
}