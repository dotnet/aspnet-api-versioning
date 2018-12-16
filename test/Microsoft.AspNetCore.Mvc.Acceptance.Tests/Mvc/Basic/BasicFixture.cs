namespace Microsoft.AspNetCore.Mvc.Basic
{
    using Microsoft.AspNetCore.Mvc.Basic.Controllers;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using System.Reflection;

    public class BasicFixture : HttpServerFixture
    {
        public BasicFixture()
        {
            FilteredControllerTypes.Add( typeof( ValuesController ).GetTypeInfo() );
            FilteredControllerTypes.Add( typeof( Values2Controller ).GetTypeInfo() );
            FilteredControllerTypes.Add( typeof( HelloWorldController ).GetTypeInfo() );
            FilteredControllerTypes.Add( typeof( HelloWorld2Controller ).GetTypeInfo() );
            FilteredControllerTypes.Add( typeof( PingController ).GetTypeInfo() );
            FilteredControllerTypes.Add( typeof( OrdersController ).GetTypeInfo() );
        }

        protected override void OnAddApiVersioning( ApiVersioningOptions options ) => options.ReportApiVersions = true;
    }
}