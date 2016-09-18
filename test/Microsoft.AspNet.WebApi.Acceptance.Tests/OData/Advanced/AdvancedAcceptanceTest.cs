namespace Microsoft.Web.OData.Advanced
{
    using Builder;
    using Configuration;
    using Controllers;
    using Http;
    using Http.Versioning;
    using System.Web.Http;
    using System.Web.OData.Builder;
    using static System.Web.Http.RouteParameter;

    public abstract class AdvancedAcceptanceTest : ODataAcceptanceTest
    {
        protected AdvancedAcceptanceTest()
        {
            FilteredControllerTypes.Add( typeof( OrdersController ) );
            FilteredControllerTypes.Add( typeof( Orders2Controller ) );
            FilteredControllerTypes.Add( typeof( Orders3Controller ) );
            FilteredControllerTypes.Add( typeof( PeopleController ) );
            FilteredControllerTypes.Add( typeof( People2Controller ) );

            Configuration.MapHttpAttributeRoutes();
            Configuration.AddApiVersioning(
                options =>
                {
                    options.ReportApiVersions = true;
                    options.AssumeDefaultVersionWhenUnspecified = true;
                    options.ApiVersionReader = new QueryStringOrHeaderApiVersionReader()
                    {
                        HeaderNames =
                        {
                            "api-version",
                            "x-ms-version"
                        }
                    };
                } );

            var modelBuilder = new VersionedODataModelBuilder( Configuration )
            {
                ModelBuilderFactory = () => new ODataConventionModelBuilder().EnableLowerCamelCase(),
                ModelConfigurations =
                {
                    new PersonModelConfiguration(),
                    new OrderModelConfiguration( supportedApiVersion: new ApiVersion( 2, 0 ) )
                }
            };
            var models = modelBuilder.GetEdmModels();

            Configuration.MapVersionedODataRoutes( "odata", "api", models );
            Configuration.Routes.MapHttpRoute( "orders", "api/{controller}/{key}", new { key = Optional } );
            Configuration.EnsureInitialized();
        }
    }
}