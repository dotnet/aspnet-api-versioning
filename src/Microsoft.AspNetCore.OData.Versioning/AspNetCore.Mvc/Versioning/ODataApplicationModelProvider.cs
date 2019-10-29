#pragma warning disable CA1812

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
            var application = context.Result;
            var conventions = new IApplicationModelConvention[]
            {
                new MetadataControllerConvention( Options ),
                new ApiExplorerModelConvention(),
            };

            for ( var i = 0; i < conventions.Length; i++ )
            {
                conventions[i].Apply( application );
            }
        }

        public void OnProvidersExecuting( ApplicationModelProviderContext context ) { }
    }
}