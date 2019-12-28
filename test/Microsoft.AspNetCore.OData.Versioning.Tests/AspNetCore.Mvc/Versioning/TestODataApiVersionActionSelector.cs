namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.ActionConstraints;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using System.Collections.Generic;

    public class TestODataApiVersionActionSelector : ODataApiVersionActionSelector
    {
        public TestODataApiVersionActionSelector(
            IActionDescriptorCollectionProvider actionDescriptorCollectionProvider,
            IEnumerable<IActionConstraintProvider> actionConstraintProviders,
            IOptions<ApiVersioningOptions> options,
            ILoggerFactory loggerFactory,
            IApiVersionRoutePolicy routePolicy )
            : base( actionDescriptorCollectionProvider, actionConstraintProviders, options, loggerFactory, routePolicy ) { }

        public override ActionDescriptor SelectBestCandidate( RouteContext context, IReadOnlyList<ActionDescriptor> candidates ) =>
            SelectedCandidate = base.SelectBestCandidate( context, candidates );

        public ActionDescriptor SelectedCandidate { get; private set; }
    }
}