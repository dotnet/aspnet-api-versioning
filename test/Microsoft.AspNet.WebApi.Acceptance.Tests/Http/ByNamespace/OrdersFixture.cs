namespace Microsoft.Web.Http.ByNamespace
{
    using Microsoft.Web.Http.Routing;
    using Microsoft.Web.Http.Versioning.Conventions;
    using System.Web.Http;
    using System.Web.Http.Routing;

    public class OrdersFixture : HttpServerFixture
    {
        public OrdersFixture()
        {
            FilteredControllerTypes.Add( typeof( Controllers.V1.OrdersController ) );
            FilteredControllerTypes.Add( typeof( Controllers.V2.OrdersController ) );
            FilteredControllerTypes.Add( typeof( Controllers.V3.OrdersController ) );

            var constraintResolver = new DefaultInlineConstraintResolver()
            {
                ConstraintMap = { ["apiVersion"] = typeof( ApiVersionRouteConstraint ) },
            };

            Configuration.MapHttpAttributeRoutes( constraintResolver );
            Configuration.AddApiVersioning(
                options =>
                {
                    options.ReportApiVersions = true;
                    options.Conventions.Add( new VersionByNamespaceConvention() );
                } );

            Configuration.EnsureInitialized();
        }
    }
}