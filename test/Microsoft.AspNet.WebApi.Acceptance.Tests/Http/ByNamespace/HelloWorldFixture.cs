namespace Microsoft.Web.Http.ByNamespace
{
    using Microsoft.Web.Http.Routing;
    using Microsoft.Web.Http.Versioning.Conventions;
    using System.Web.Http;
    using System.Web.Http.Routing;

    public class HelloWorldFixture : HttpServerFixture
    {
        public HelloWorldFixture()
        {
            FilteredControllerTypes.Add( typeof( Controllers.V1.HelloWorldController ) );
            FilteredControllerTypes.Add( typeof( Controllers.V2.HelloWorldController ) );
            FilteredControllerTypes.Add( typeof( Controllers.V3.HelloWorldController ) );

            var constraintResolver = new DefaultInlineConstraintResolver()
            {
                ConstraintMap = { ["apiVersion"] = typeof( ApiVersionRouteConstraint ) },
            };

            Configuration.MapHttpAttributeRoutes( constraintResolver );
            Configuration.AddApiVersioning(
                options =>
                {
                    options.ReportApiVersions = true;
                    options.DefaultApiVersion = new ApiVersion( 2, 0 );
                    options.AssumeDefaultVersionWhenUnspecified = true;
                    options.Conventions.Add( new VersionByNamespaceConvention() );
                } );

            Configuration.EnsureInitialized();
        }
    }
}