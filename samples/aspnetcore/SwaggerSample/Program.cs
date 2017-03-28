namespace Microsoft.Examples
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using System.IO;

    /// <summary>
    /// Represents the current application.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The main entry point to the application.
        /// </summary>
        /// <param name="args">The arguments provides at start-up, if any.</param>
        public static void Main( string[] args )
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot( Directory.GetCurrentDirectory() )
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}