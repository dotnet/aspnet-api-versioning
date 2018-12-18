namespace Microsoft.Examples
{
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;

    /// <summary>
    /// Represents the current application.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point to the application.
        /// </summary>
        /// <param name="args">The arguments provides at start-up, if any.</param>
        public static void Main( string[] args ) =>
            CreateWebHostBuilder( args ).Build().Run();

        /// <summary>
        /// Builds a new web host for the application.
        /// </summary>
        /// <param name="args">The command-line arguments, if any.</param>
        /// <returns>A new <see cref="IWebHostBuilder">web host builder</see>.</returns>
        public static IWebHostBuilder CreateWebHostBuilder( string[] args ) =>
            WebHost.CreateDefaultBuilder( args )
                   .UseStartup<Startup>();
    }
}