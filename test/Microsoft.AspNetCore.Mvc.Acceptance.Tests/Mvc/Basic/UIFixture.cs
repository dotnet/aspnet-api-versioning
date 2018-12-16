namespace Microsoft.AspNetCore.Mvc.Basic
{
    using Microsoft.AspNetCore.Mvc.ApplicationParts;
    using Microsoft.AspNetCore.Mvc.Razor;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.Extensions.DependencyInjection;
    using System.Linq;
    using System.Reflection;
    using static Microsoft.CodeAnalysis.MetadataReference;
    using static System.Reflection.Assembly;

    public class UIFixture : HttpServerFixture
    {
        protected override void OnAddApiVersioning( ApiVersioningOptions options ) => options.ReportApiVersions = true;

        protected override void OnConfigurePartManager( ApplicationPartManager partManager )
        {
            partManager.FeatureProviders.Add( (IApplicationFeatureProvider) FilteredControllerTypes );
            partManager.ApplicationParts.Add( new AssemblyPart( GetType().Assembly ) );
        }

        protected override void OnConfigureServices( IServiceCollection services )
        {
            services.Configure<RazorViewEngineOptions>( options =>
            {
                options.CompilationCallback += context =>
                {
                    var assembly = GetType().GetTypeInfo().Assembly;
                    var assemblies = assembly.GetReferencedAssemblies().Select( x => CreateFromFile( Load( x ).Location ) ).ToList();

                    assemblies.Add( CreateFromFile( Load( new AssemblyName( "mscorlib" ) ).Location ) );
#if NET461
                    assemblies.Add( CreateFromFile( Load( new AssemblyName( "netstandard" ) ).Location ) );
#else
                    assemblies.Add( CreateFromFile( Load( new AssemblyName( "System.Private.Corelib" ) ).Location ) );
#endif
                    assemblies.Add( CreateFromFile( Load( new AssemblyName( "System.Dynamic.Runtime" ) ).Location ) );
                    assemblies.Add( CreateFromFile( Load( new AssemblyName( "Microsoft.AspNetCore.Razor" ) ).Location ) );
                    context.Compilation = context.Compilation.AddReferences( assemblies );
                };
            } );
        }
    }
}