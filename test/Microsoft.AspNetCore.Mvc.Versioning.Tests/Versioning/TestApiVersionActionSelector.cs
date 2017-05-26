namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Abstractions;
    using AspNetCore.Routing;
    using Extensions.Logging;
    using Extensions.Options;
    using Internal;
    using System.Collections.Generic;
    using System.Linq;

    public class TestApiVersionActionSelector : ApiVersionActionSelector
    {
        public TestApiVersionActionSelector(
            IActionSelectorDecisionTreeProvider decisionTreeProvider,
            ActionConstraintCache actionConstraintCache,
            IOptions<ApiVersioningOptions> options,
            ILoggerFactory loggerFactory )
            : base( decisionTreeProvider, actionConstraintCache, options, loggerFactory ) { }

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