{{#include ../../shared/odata/overview-pre.md}}

```c#
public class Startup
{
    public void Configuration( IAppBuilder appBuilder )
    {
        var configuration = new HttpConfiguration();
        var httpServer = new HttpServer( configuration );

        configuration.AddApiVersioning();

        var modelBuilder = new VersionedODataModelBuilder( configuration )
        {
            ModelConfigurations =
            {
                new PersonModelConfiguration()
            }
        };

        configuration.MapVersionedODataRoute( "odata", "api", modelBuilder );
        appBuilder.UseWebApi( httpServer );
    }
}
```