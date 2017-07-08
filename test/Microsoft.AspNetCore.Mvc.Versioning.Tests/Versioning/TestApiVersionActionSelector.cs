namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Abstractions;
    using AspNetCore.Routing;
    using Extensions.Logging;
    using Extensions.Options;
    using Internal;
    using Infrastructure;
    using System.Collections.Generic;
    using System.Linq;

    public class TestApiVersionActionSelector : ApiVersionActionSelector
    {
        public TestApiVersionActionSelector(
            IActionDescriptorCollectionProvider actionDescriptorCollectionProvider,
            ActionConstraintCache actionConstraintCache,
            IOptions<ApiVersioningOptions> options,
            ILoggerFactory loggerFactory )
            : base( actionDescriptorCollectionProvider, actionConstraintCache, options, loggerFactory ) { }

        public override ActionDescriptor SelectBestCandidate( RouteContext context, IReadOnlyList<ActionDescriptor> candidates )
        {
            var bestCandidate = base.SelectBestCandidate( context, candidates );
            var selectionResult = context.HttpContext.ApiVersionProperties().SelectionResult;

            SelectedCandidate = selectionResult.BestMatch?.Action;

            return bestCandidate;
        }

        public ActionDescriptor SelectedCandidate { get; private set; }
    }
}