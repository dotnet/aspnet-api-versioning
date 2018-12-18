namespace Microsoft.Examples
{
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;

    public static class Program
    {
        public static void Main( string[] args ) =>
            CreateWebHostBuilder( args ).Build().Run();

        public static IWebHostBuilder CreateWebHostBuilder( string[] args ) =>
            WebHost.CreateDefaultBuilder( args )
                   .UseStartup<Startup>();
    }
}