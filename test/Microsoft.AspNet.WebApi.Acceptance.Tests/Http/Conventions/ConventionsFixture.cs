namespace Microsoft.Web.Http.Conventions
{
    using Microsoft.Web.Http.Conventions.Controllers;
    using Microsoft.Web.Http.Routing;
    using Microsoft.Web.Http.Versioning.Conventions;
    using System.Web.Http;
    using System.Web.Http.Routing;

    public class ConventionsFixture : HttpServerFixture
    {
        public ConventionsFixture()
        {
            var constraintResolver = new DefaultInlineConstraintResolver()
            {
                ConstraintMap = { ["apiVersion"] = typeof( ApiVersionRouteConstraint ) },
            };

            FilteredControllerTypes.Add( typeof( ValuesController ) );
            FilteredControllerTypes.Add( typeof( Values2Controller ) );
            FilteredControllerTypes.Add( typeof( HelloWorldController ) );
            FilteredControllerTypes.Add( typeof( HelloWorld2Controller ) );
            FilteredControllerTypes.Add( typeof( OrdersController ) );
            Configuration.AddApiVersioning(
                options =>
                {
                    options.ReportApiVersions = true;
                    options.Conventions.Controller<ValuesController>().HasApiVersion( 1, 0 );
                    options.Conventions.Controller<Values2Controller>()
                                       .HasApiVersion( 2, 0 )
                                       .HasApiVersion( 3, 0 )
                                       .Action( c => c.GetV3() ).MapToApiVersion( 3, 0 )
                                       .Action( c => c.GetV3( default ) ).MapToApiVersion( 3, 0 );
                    options.Conventions.Controller<HelloWorldController>().HasDeprecatedApiVersion( 1, 0 );
                    options.Conventions.Controller<HelloWorld2Controller>()
                                       .HasApiVersion( 2, 0 )
                                       .HasApiVersion( 3, 0 )
                                       .AdvertisesApiVersion( 4, 0 )
                                       .Action( c => c.GetV3() ).MapToApiVersion( 3, 0 )
                                       .Action( c => c.GetV3( default ) ).MapToApiVersion( 3, 0 );
                    options.Conventions.Controller<OrdersController>()
                                       .Action( c => c.Get() ).HasApiVersion( 1, 0 ).HasApiVersion( 2, 0 )
                                       .Action( c => c.Get( default ) ).HasApiVersion( 0, 9 ).HasApiVersion( 1, 0 ).HasApiVersion( 2, 0 )
                                       .Action( c => c.Post( default ) ).HasApiVersion( 1, 0 ).HasApiVersion( 2, 0 )
                                       .Action( c => c.Put( default, default ) ).HasApiVersion( 2, 0 )
                                       .Action( c => c.Delete( default ) ).IsApiVersionNeutral();
                } );
            Configuration.MapHttpAttributeRoutes( constraintResolver );
            Configuration.EnsureInitialized();
        }
    }
}