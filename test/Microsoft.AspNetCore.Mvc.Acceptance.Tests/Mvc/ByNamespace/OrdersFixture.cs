namespace Microsoft.AspNetCore.Mvc.ByNamespace
{
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.AspNetCore.Mvc.Versioning.Conventions;
    using System.Reflection;

    public class OrdersFixture : HttpServerFixture
    {
        public OrdersFixture()
        {
            FilteredControllerTypes.Add( typeof( Controllers.V1.OrdersController ).GetTypeInfo() );
            FilteredControllerTypes.Add( typeof( Controllers.V2.OrdersController ).GetTypeInfo() );
            FilteredControllerTypes.Add( typeof( Controllers.V3.OrdersController ).GetTypeInfo() );
        }

        protected override void OnAddApiVersioning( ApiVersioningOptions options )
        {
            options.ReportApiVersions = true;
            options.Conventions.Add( new VersionByNamespaceConvention() );
        }
    }
}