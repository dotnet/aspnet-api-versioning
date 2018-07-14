namespace Microsoft.Examples
{
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;

    /// <summary>
    /// Represents the current application.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The main entry point to the application.
        /// </summary>
        /// <param name="args">The arguments provides at start-up, if any.</param>
        public static void Main( string[] args ) => BuildWebHost( args ).Run();

        /// <summary>
        /// Builds a new web host for the application.
        /// </summary>
        /// <param name="args">The command-line arguments, if any.</param>
        /// <returns>A newly constructed <see cref="IWebHost">web host</see>.</returns>
        public static IWebHost BuildWebHost( string[] args ) =>
            WebHost.CreateDefaultBuilder( args )
                .UseStartup<Startup>()
                .Build();
    }
}