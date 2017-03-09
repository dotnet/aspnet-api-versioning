namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Abstractions;
    using AspNetCore.Routing;
    using Extensions.Logging;
    using Extensions.Options;
    using Internal;
    using System.Collections.Generic;

    public class TestApiVersionActionSelector : ApiVersionActionSelector
    {
        public TestApiVersionActionSelector(
            IActionSelectorDecisionTreeProvider decisionTreeProvider,
            ActionConstraintCache actionConstraintCache,
            IOptions<ApiVersioningOptions> options,
            ILoggerFactory loggerFactory )
            : base( decisionTreeProvider, actionConstraintCache, options, loggerFactory )
        {
        }

        public override ActionDescriptor SelectBestCandidate( RouteContext context, IReadOnlyList<ActionDescriptor> candidates ) =>
            SelectedCandidate = base.SelectBestCandidate( context, candidates );

        public ActionDescriptor SelectedCandidate { get; private set; }
    }
}