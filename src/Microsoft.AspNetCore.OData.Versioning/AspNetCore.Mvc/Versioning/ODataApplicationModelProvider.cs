namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Microsoft.AspNetCore.Mvc.ApplicationModels;
    using Microsoft.Extensions.Options;

    sealed class ODataApplicationModelProvider : IApplicationModelProvider
    {
        const int AfterApiVersioningConventions = -100;
        readonly IOptions<ODataApiVersioningOptions> options;

        public ODataApplicationModelProvider( IOptions<ODataApiVersioningOptions> options ) => this.options = options;

        private ODataApiVersioningOptions Options => options.Value;

        public int Order => AfterApiVersioningConventions;

        public void OnProvidersExecuted( ApplicationModelProviderContext context )
        {
            var convention = new MetadataControllerConvention( Options );
            convention.Apply( context.Result );
        }

        public void OnProvidersExecuting( ApplicationModelProviderContext context ) { }
    }
}