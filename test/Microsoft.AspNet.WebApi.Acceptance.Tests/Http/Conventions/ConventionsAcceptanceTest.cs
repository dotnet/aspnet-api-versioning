namespace Microsoft.Web.Http.Conventions
{
    using Controllers;
    using Microsoft.Web.Http.Routing;
    using System.Web.Http;
    using System.Web.Http.Routing;
    using Versioning.Conventions;

    public abstract class ConventionsAcceptanceTest : AcceptanceTest
    {
        protected ConventionsAcceptanceTest()
        {
            var constraintResolver = new DefaultInlineConstraintResolver()
            {
                ConstraintMap = { ["apiVersion"] = typeof( ApiVersionRouteConstraint ) }
            };

            FilteredControllerTypes.Add( typeof( ValuesController ) );
            FilteredControllerTypes.Add( typeof( Values2Controller ) );
            FilteredControllerTypes.Add( typeof( HelloWorldController ) );
            FilteredControllerTypes.Add( typeof( HelloWorld2Controller ) );
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
                } );
            Configuration.MapHttpAttributeRoutes( constraintResolver );
            Configuration.EnsureInitialized();
        }
    }
}