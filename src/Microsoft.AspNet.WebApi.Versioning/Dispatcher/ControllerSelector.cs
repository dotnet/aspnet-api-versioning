namespace Microsoft.Web.Http.Dispatcher
{
    using System.Diagnostics.Contracts;
    using Microsoft.Web.Http.Versioning;

    abstract class ControllerSelector
    {
        readonly ApiVersioningOptions options;

        protected ControllerSelector( ApiVersioningOptions options )
        {
            Contract.Requires( options != null );
            this.options = options;
        }

        protected bool AssumeDefaultVersionWhenUnspecified => options.AssumeDefaultVersionWhenUnspecified;

        protected IApiVersionSelector ApiVersionSelector => options.ApiVersionSelector;

        internal abstract ControllerSelectionResult SelectController( ApiVersionControllerAggregator aggregator );
    }
}