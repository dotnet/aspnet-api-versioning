namespace Microsoft.AspNet.OData.Advanced
{
    using Microsoft.AspNet.OData.Advanced.Controllers;
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.AspNet.OData.Configuration;
    using Microsoft.AspNet.OData.Routing;
    using Microsoft.OData;
    using Microsoft.OData.UriParser;
    using Microsoft.Web.Http;
    using Microsoft.Web.Http.Versioning;
    using System.Web.Http;
    using static Microsoft.OData.ServiceLifetime;
    using static System.Web.Http.RouteParameter;

    public class AdvancedFixture : ODataFixture
    {
        public AdvancedFixture()
        {
            FilteredControllerTypes.Add( typeof( OrdersController ) );
            FilteredControllerTypes.Add( typeof( Orders2Controller ) );
            FilteredControllerTypes.Add( typeof( Orders3Controller ) );
            FilteredControllerTypes.Add( typeof( PeopleController ) );
            FilteredControllerTypes.Add( typeof( People2Controller ) );

            Configuration.AddApiVersioning(
                options =>
                {
                    options.ReportApiVersions = true;
                    options.AssumeDefaultVersionWhenUnspecified = true;
                    options.ApiVersionReader = ApiVersionReader.Combine(
                        new QueryStringApiVersionReader(),
                        new HeaderApiVersionReader( "api-version", "x-ms-version" ) );
                } );

            var modelBuilder = new VersionedODataModelBuilder( Configuration )
            {
                ModelConfigurations =
                {
                    new PersonModelConfiguration(),
                    new OrderModelConfiguration( supportedApiVersion: new ApiVersion( 2, 0 ) ),
                }
            };
            var models = modelBuilder.GetEdmModels();

            Configuration.MapVersionedODataRoutes( "odata", "api", models, OnConfigureContainer );
            Configuration.Routes.MapHttpRoute( "orders", "api/{controller}/{key}", new { key = Optional } );
            Configuration.EnsureInitialized();
        }

        void OnConfigureContainer( IContainerBuilder builder )
        {
            builder.AddService( Singleton, typeof( ODataUriResolver ), sp => UriResolver );
            builder.AddService( Singleton, typeof( IODataPathHandler ), sp => PathHandler );
        }
    }
}