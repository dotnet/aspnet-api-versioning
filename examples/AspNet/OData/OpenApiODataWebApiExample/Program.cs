namespace ApiVersioning.Examples;

using Microsoft.Owin.Hosting;
using System.Diagnostics;

/// <summary>
/// Represents the current application.
/// </summary>
public class Program
{
    private const string Url = "http://localhost:9007/";
    private const string LaunchUrl = Url + "swagger";
    private static readonly ManualResetEvent resetEvent = new( false );

    /// <summary>
    /// The main entry point to the application.
    /// </summary>
    /// <param name="args">The arguments provided at start-up, if any.</param>
    public static void Main( string[] args )
    {
        Console.CancelKeyPress += OnCancel;

        using ( WebApp.Start<Startup>( Url ) )
        {
            Console.WriteLine( "Content root path: " + Startup.ContentRootPath );
            Console.WriteLine( "Now listening on: " + Url );
            Console.WriteLine( "Application started. Press Ctrl+C to shut down." );
            Process.Start( LaunchUrl );
            resetEvent.WaitOne();
        }

        Console.CancelKeyPress -= OnCancel;
    }

    private static void OnCancel( object sender, ConsoleCancelEventArgs e )
    {
        Console.Write( "Application is shutting down..." );
        e.Cancel = true;
        resetEvent.Set();
    }
}