namespace Microsoft.Web.Http.ByNamespace
{
    using Microsoft.Web.Http.Routing;
    using System.Web.Http;
    using static System.Web.Http.RouteParameter;

    public abstract class ByNamespaceAcceptanceTest : AcceptanceTest
    {
        protected ByNamespaceAcceptanceTest()
        {
            FilteredControllerTypes.Add( typeof( Controllers.V1.AgreementsController ) );
            FilteredControllerTypes.Add( typeof( Controllers.V2.AgreementsController ) );
            FilteredControllerTypes.Add( typeof( Controllers.V3.AgreementsController ) );

            Configuration.AddApiVersioning( options => options.ReportApiVersions = true );

            Configuration.Routes.MapHttpRoute(
                "VersionedQueryString",
                "api/{controller}/{accountId}",
                new { accountId = Optional } );

            Configuration.Routes.MapHttpRoute(
                "VersionedUrl",
                "v{apiVersion}/{controller}/{accountId}",
                new { accountId = Optional },
                new { apiVersion = new ApiVersionRouteConstraint() } );

            Configuration.EnsureInitialized();
        }
    }
}