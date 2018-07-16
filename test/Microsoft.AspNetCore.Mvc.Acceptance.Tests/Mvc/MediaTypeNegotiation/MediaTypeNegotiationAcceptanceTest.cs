namespace Microsoft.AspNetCore.Mvc.MediaTypeNegotiation
{
    using Controllers;
    using System.Reflection;
    using Versioning;

    public abstract class MediaTypeNegotiationAcceptanceTest  : AcceptanceTest
    {
        protected MediaTypeNegotiationAcceptanceTest()
        {
            FilteredControllerTypes.Add( typeof( ValuesController ).GetTypeInfo() );
            FilteredControllerTypes.Add( typeof( Values2Controller ).GetTypeInfo() );
            FilteredControllerTypes.Add( typeof( HelloWorldController ).GetTypeInfo() );
        }

        protected override void OnAddApiVersioning( ApiVersioningOptions options )
        {
            options.ApiVersionReader = new MediaTypeApiVersionReader();
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ApiVersionSelector = new CurrentImplementationApiVersionSelector( options );
            options.ReportApiVersions = true;
        }
    }
}