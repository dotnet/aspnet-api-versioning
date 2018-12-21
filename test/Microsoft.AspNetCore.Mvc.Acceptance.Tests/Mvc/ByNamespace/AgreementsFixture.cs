namespace Microsoft.AspNetCore.Mvc.ByNamespace
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.AspNetCore.Mvc.Versioning.Conventions;
    using Microsoft.AspNetCore.Routing;
    using System.Reflection;

    public class AgreementsFixture : HttpServerFixture
    {
        public AgreementsFixture()
        {
            FilteredControllerTypes.Add( typeof( Controllers.V1.AgreementsController ).GetTypeInfo() );
            FilteredControllerTypes.Add( typeof( Controllers.V2.AgreementsController ).GetTypeInfo() );
            FilteredControllerTypes.Add( typeof( Controllers.V3.AgreementsController ).GetTypeInfo() );
        }

        protected override void OnAddApiVersioning( ApiVersioningOptions options )
        {
            options.ReportApiVersions = true;
            options.UseApiBehavior = false;
            options.Conventions.Add( new VersionByNamespaceConvention() );
        }

        protected override void OnConfigureRoutes( IRouteBuilder routeBuilder )
        {
            routeBuilder.MapRoute( "VersionedQueryString", "api/{controller}/{accountId}/{action=Get}" );
            routeBuilder.MapRoute( "VersionedUrl", "v{version:apiVersion}/{controller}/{accountId}/{action=Get}" );
        }
    }
}