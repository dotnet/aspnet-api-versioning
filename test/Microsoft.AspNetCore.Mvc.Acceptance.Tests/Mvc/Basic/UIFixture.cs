namespace Microsoft.AspNetCore.Mvc.Basic
{
    using Microsoft.AspNetCore.Mvc.ApplicationParts;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.Extensions.DependencyInjection;

    public class UIFixture : HttpServerFixture
    {
        protected override void OnAddApiVersioning( ApiVersioningOptions options ) => options.ReportApiVersions = true;

        protected override void OnConfigurePartManager( ApplicationPartManager partManager )
        {
            partManager.FeatureProviders.Add( (IApplicationFeatureProvider) FilteredControllerTypes );
            partManager.ApplicationParts.Add( new AssemblyPart( GetType().Assembly ) );
        }

#if !NET461
        protected override void OnConfigureServices( IServiceCollection services ) =>
            services.AddControllersWithViews().AddRazorRuntimeCompilation();
#endif
    }
}