namespace Microsoft.AspNetCore.Mvc.Conventions
{
    using Microsoft.AspNetCore.Mvc.Conventions.Controllers;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.AspNetCore.Mvc.Versioning.Conventions;
    using System.Reflection;

    public class ConventionsFixture : HttpServerFixture
    {
        public ConventionsFixture()
        {
            FilteredControllerTypes.Add( typeof( ValuesController ).GetTypeInfo() );
            FilteredControllerTypes.Add( typeof( Values2Controller ).GetTypeInfo() );
            FilteredControllerTypes.Add( typeof( HelloWorldController ).GetTypeInfo() );
            FilteredControllerTypes.Add( typeof( HelloWorld2Controller ).GetTypeInfo() );
            FilteredControllerTypes.Add( typeof( OrdersController ).GetTypeInfo() );
        }

        protected override void OnAddApiVersioning( ApiVersioningOptions options )
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
        }
    }
}