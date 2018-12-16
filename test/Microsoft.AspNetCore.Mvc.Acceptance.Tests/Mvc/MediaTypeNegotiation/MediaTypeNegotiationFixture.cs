namespace Microsoft.AspNetCore.Mvc.MediaTypeNegotiation
{
    using Microsoft.AspNetCore.Mvc.MediaTypeNegotiation.Controllers;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using System.Reflection;

    public class MediaTypeNegotiationFixture : HttpServerFixture
    {
        public MediaTypeNegotiationFixture()
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