namespace Microsoft.Web.Http.MediaTypeNegotiation
{
    using Controllers;
    using System.Web.Http;
    using Versioning;

    public abstract class MediaTypeNegotiationAcceptanceTest : AcceptanceTest
    {
        protected MediaTypeNegotiationAcceptanceTest()
        {
            FilteredControllerTypes.Add( typeof( ValuesController ) );
            FilteredControllerTypes.Add( typeof( Values2Controller ) );
            FilteredControllerTypes.Add( typeof( HelloWorldController ) );
            Configuration.AddApiVersioning(
                options =>
                {
                    options.ApiVersionReader = new MediaTypeApiVersionReader();
                    options.AssumeDefaultVersionWhenUnspecified = true;
                    options.ApiVersionSelector = new CurrentImplementationApiVersionSelector( options );
                    options.ReportApiVersions = true;
                } );
            Configuration.MapHttpAttributeRoutes();
            Configuration.EnsureInitialized();
        }
    }
}