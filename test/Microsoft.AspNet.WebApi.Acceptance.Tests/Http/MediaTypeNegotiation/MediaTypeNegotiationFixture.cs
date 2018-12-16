namespace Microsoft.Web.Http.MediaTypeNegotiation
{
    using Microsoft.Web.Http.MediaTypeNegotiation.Controllers;
    using Microsoft.Web.Http.Versioning;
    using System.Web.Http;

    public class MediaTypeNegotiationFixture : HttpServerFixture
    {
        public MediaTypeNegotiationFixture()
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