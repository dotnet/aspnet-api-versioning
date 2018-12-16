namespace Microsoft.AspNetCore.Mvc.Basic
{
    using Microsoft.AspNetCore.Mvc.Basic.Controllers;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using System.Reflection;

    public class OverlappingRouteTemplateFixture : HttpServerFixture
    {
        public OverlappingRouteTemplateFixture()
        {
            FilteredControllerTypes.Add( typeof( OverlappingRouteTemplateController ).GetTypeInfo() );
        }

        protected override void OnAddApiVersioning( ApiVersioningOptions options ) => options.ReportApiVersions = true;
    }
}