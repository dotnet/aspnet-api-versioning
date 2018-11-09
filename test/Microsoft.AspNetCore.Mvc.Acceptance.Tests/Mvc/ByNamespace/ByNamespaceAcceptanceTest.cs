namespace Microsoft.AspNetCore.Mvc.ByNamespace
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.AspNetCore.Mvc.Versioning.Conventions;
    using Microsoft.AspNetCore.Routing;
    using System.Reflection;

    public abstract class ByNamespaceAcceptanceTest : AcceptanceTest
    {
        readonly SetupKind kind;

        protected ByNamespaceAcceptanceTest( SetupKind kind )
        {
            this.kind = kind;

            switch ( kind )
            {
                case SetupKind.Agreements:
                    ConfigureAgreements();
                    break;
                case SetupKind.Orders:
                    ConfigureOrders();
                    break;
            }
        }

        protected override void OnConfigureRoutes( IRouteBuilder routeBuilder )
        {
            switch ( kind )
            {
                case SetupKind.Agreements:
                    routeBuilder.MapRoute( "VersionedQueryString", "api/{controller}/{accountId}/{action=Get}" );
                    routeBuilder.MapRoute( "VersionedUrl", "v{version:apiVersion}/{controller}/{accountId}/{action=Get}" );
                    break;
            }
        }

        protected override void OnAddApiVersioning( ApiVersioningOptions options )
        {
            options.ReportApiVersions = true;
            options.Conventions.Add( new VersionByNamespaceConvention() );
        }

        void ConfigureAgreements()
        {
            FilteredControllerTypes.Add( typeof( Controllers.V1.AgreementsController ).GetTypeInfo() );
            FilteredControllerTypes.Add( typeof( Controllers.V2.AgreementsController ).GetTypeInfo() );
            FilteredControllerTypes.Add( typeof( Controllers.V3.AgreementsController ).GetTypeInfo() );
        }

        void ConfigureOrders()
        {
            FilteredControllerTypes.Add( typeof( Controllers.V1.OrdersController ).GetTypeInfo() );
            FilteredControllerTypes.Add( typeof( Controllers.V2.OrdersController ).GetTypeInfo() );
            FilteredControllerTypes.Add( typeof( Controllers.V3.OrdersController ).GetTypeInfo() );
        }

        protected enum SetupKind
        {
            None,
            Agreements,
            Orders,
        }
    }
}