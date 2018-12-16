namespace Microsoft.Web.Http.Basic
{
    using Microsoft.Web.Http.Basic.Controllers;
    using Microsoft.Web.Http.Routing;
    using System.Web.Http;
    using System.Web.Http.Routing;

    public class BasicFixture : HttpServerFixture
    {
        public BasicFixture()
        {
            var constraintResolver = new DefaultInlineConstraintResolver()
            {
                ConstraintMap = { ["apiVersion"] = typeof( ApiVersionRouteConstraint ) }
            };

            FilteredControllerTypes.Add( typeof( ValuesController ) );
            FilteredControllerTypes.Add( typeof( Values2Controller ) );
            FilteredControllerTypes.Add( typeof( HelloWorldController ) );
            FilteredControllerTypes.Add( typeof( PingController ) );
            FilteredControllerTypes.Add( typeof( OverlappingRouteTemplateController ) );
            FilteredControllerTypes.Add( typeof( OrdersController ) );
            Configuration.AddApiVersioning( options => options.ReportApiVersions = true );
            Configuration.MapHttpAttributeRoutes( constraintResolver );
            Configuration.EnsureInitialized();
        }
    }
}