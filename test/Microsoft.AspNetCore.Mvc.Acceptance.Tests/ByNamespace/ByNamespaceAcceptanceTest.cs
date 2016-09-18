namespace Microsoft.AspNetCore.Mvc.ByNamespace
{
    using AspNetCore.Routing;
    using Builder;
    using System.Reflection;
    using Versioning;

    public abstract class ByNamespaceAcceptanceTest : AcceptanceTest
    {
        protected ByNamespaceAcceptanceTest()
        {
            FilteredControllerTypes.Add( typeof( Controllers.V1.AgreementsController ).GetTypeInfo() );
            FilteredControllerTypes.Add( typeof( Controllers.V2.AgreementsController ).GetTypeInfo() );
            FilteredControllerTypes.Add( typeof( Controllers.V3.AgreementsController ).GetTypeInfo() );
        }

        protected override void OnConfigureRoutes( IRouteBuilder routeBuilder )
        {
            routeBuilder.MapRoute( "VersionedQueryString", "api/{controller}/{accountId}/{action=Get}" );
            routeBuilder.MapRoute( "VersionedUrl", "v{version:apiVersion}/{controller}/{accountId}/{action=Get}" );
        }

        protected override void OnAddApiVersioning( ApiVersioningOptions options ) => options.ReportApiVersions = true;
    }
}