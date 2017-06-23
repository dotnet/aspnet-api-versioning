namespace Microsoft.Web.Http.Basic
{
    using Controllers;
    using Microsoft.Web.Http.Routing;
    using System.Web.Http;
    using System.Web.Http.Routing;

    public abstract class BasicAcceptanceTest : AcceptanceTest
    {
        protected BasicAcceptanceTest()
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
            Configuration.AddApiVersioning( options => options.ReportApiVersions = true );
            Configuration.MapHttpAttributeRoutes( constraintResolver );
            Configuration.EnsureInitialized();
        }
    }
}