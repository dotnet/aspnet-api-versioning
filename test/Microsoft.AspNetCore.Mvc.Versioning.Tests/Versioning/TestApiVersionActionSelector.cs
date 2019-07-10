namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
#if !NETCOREAPP
    using Microsoft.AspNetCore.Mvc.Internal;
#endif
    using Microsoft.AspNetCore.Mvc.ActionConstraints;
    using Microsoft.AspNetCore.Routing;
    using System.Collections.Generic;

    public class TestApiVersionActionSelector : ApiVersionActionSelector
    {
#if !NETCOREAPP
        public TestApiVersionActionSelector(
            IActionDescriptorCollectionProvider actionDescriptorCollectionProvider,
            ActionConstraintCache actionConstraintCache,
            IOptions<ApiVersioningOptions> options,
            ILoggerFactory loggerFactory,
            IApiVersionRoutePolicy routePolicy )
            : base( actionDescriptorCollectionProvider, actionConstraintCache, options, loggerFactory, routePolicy ) { }
#else
        public TestApiVersionActionSelector(
            IActionDescriptorCollectionProvider actionDescriptorCollectionProvider,
            IEnumerable<IActionConstraintProvider> actionConstraintProviders,
            IOptions<ApiVersioningOptions> options,
            ILoggerFactory loggerFactory,
            IApiVersionRoutePolicy routePolicy )
            : base( actionDescriptorCollectionProvider, actionConstraintProviders, options, loggerFactory, routePolicy ) { }
#endif

        public override ActionDescriptor SelectBestCandidate( RouteContext context, IReadOnlyList<ActionDescriptor> candidates ) =>
            SelectedCandidate = base.SelectBestCandidate( context, candidates );

        public ActionDescriptor SelectedCandidate { get; private set; }
    }
}