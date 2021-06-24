#pragma warning disable CA1812

namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNetCore.Mvc.ApplicationModels;
    using Microsoft.Extensions.Options;

    sealed class ODataApplicationModelProvider : IApplicationModelProvider
    {
        readonly IOptions<ODataApiVersioningOptions> options;
        internal const int AfterApiVersioningConventions = -100;

        public ODataApplicationModelProvider( IOptions<ODataApiVersioningOptions> options ) => this.options = options;

        private ODataApiVersioningOptions Options => options.Value;

        public int Order => AfterApiVersioningConventions;

        public void OnProvidersExecuted( ApplicationModelProviderContext context )
        {
            var application = context.Result;
            var applicationConventions = new IApplicationModelConvention[]
            {
                new MetadataControllerConvention( Options ),
            };

            for ( var i = 0; i < applicationConventions.Length; i++ )
            {
                applicationConventions[i].Apply( application );
            }

            var controllers = application.Controllers;
            var controllerConventions = new IControllerModelConvention[]
            {
                new ApiExplorerModelConvention(),
                new ControllerNameOverrideConvention(),
            };

            for ( var i = 0; i < controllers.Count; i++ )
            {
                var controller = controllers[i];

                if ( !controller.ControllerType.IsODataController() )
                {
                    continue;
                }

                for ( var j = 0; j < controllerConventions.Length; j++ )
                {
                    controllerConventions[j].Apply( controller );
                }
            }
        }

        public void OnProvidersExecuting( ApplicationModelProviderContext context ) { }
    }
}