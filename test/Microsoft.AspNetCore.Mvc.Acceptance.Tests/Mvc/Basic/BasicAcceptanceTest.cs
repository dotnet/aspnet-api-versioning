namespace Microsoft.AspNetCore.Mvc.Basic
{
    using Controllers;
    using System.Reflection;
    using Versioning;

    public abstract class BasicAcceptanceTest: AcceptanceTest
    {
        protected BasicAcceptanceTest()
        {
            FilteredControllerTypes.Add( typeof( ValuesController ).GetTypeInfo() );
            FilteredControllerTypes.Add( typeof( Values2Controller ).GetTypeInfo() );
            FilteredControllerTypes.Add( typeof( HelloWorldController ).GetTypeInfo() );
            FilteredControllerTypes.Add( typeof( HelloWorld2Controller ).GetTypeInfo() );
            FilteredControllerTypes.Add( typeof( PingController ).GetTypeInfo() );
        }

        protected override void OnAddApiVersioning( ApiVersioningOptions options ) => options.ReportApiVersions = true;
    }
}