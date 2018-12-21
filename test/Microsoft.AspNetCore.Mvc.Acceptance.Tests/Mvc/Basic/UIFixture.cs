namespace Microsoft.AspNetCore.Mvc.Basic
{
    using Microsoft.AspNetCore.Mvc.ApplicationParts;
    using Microsoft.AspNetCore.Mvc.Versioning;

    public class UIFixture : HttpServerFixture
    {
        protected override void OnAddApiVersioning( ApiVersioningOptions options ) => options.ReportApiVersions = true;

        protected override void OnConfigurePartManager( ApplicationPartManager partManager )
        {
            partManager.FeatureProviders.Add( (IApplicationFeatureProvider) FilteredControllerTypes );
            partManager.ApplicationParts.Add( new AssemblyPart( GetType().Assembly ) );
        }
    }
}