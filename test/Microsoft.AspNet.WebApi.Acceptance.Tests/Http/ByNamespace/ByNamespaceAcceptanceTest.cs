namespace Microsoft.Web.Http.ByNamespace
{
    using Microsoft.Web.Http.Routing;
    using System.Web.Http;
    using System.Web.Http.Routing;
    using static System.Web.Http.RouteParameter;

    public abstract class ByNamespaceAcceptanceTest : AcceptanceTest
    {
        protected enum SetupKind
        {
            None,
            HelloWorld,
            Agreements
        }

        protected ByNamespaceAcceptanceTest( SetupKind kind )
        {
            switch ( kind )
            {
                case SetupKind.HelloWorld:
                    ConfigureHelloWorld();
                    break;
                case SetupKind.Agreements:
                    ConfigureAgreements();
                    break;
            }

            Configuration.EnsureInitialized();
        }

        void ConfigureAgreements()
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

        }

        void ConfigureHelloWorld()
        {
            FilteredControllerTypes.Add( typeof( Controllers.V1.HelloWorldController ) );
            FilteredControllerTypes.Add( typeof( Controllers.V2.HelloWorldController ) );
            FilteredControllerTypes.Add( typeof( Controllers.V3.HelloWorldController ) );

            var constraintResolver = new DefaultInlineConstraintResolver()
            {
                ConstraintMap = { ["apiVersion"] = typeof( ApiVersionRouteConstraint ) }
            };

            Configuration.MapHttpAttributeRoutes( constraintResolver );
            Configuration.AddApiVersioning(
                options =>
                {
                    options.ReportApiVersions = true;
                    options.DefaultApiVersion = new ApiVersion( 2, 0 );
                    options.AssumeDefaultVersionWhenUnspecified = true;
                } );
        }
    }
}