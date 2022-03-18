namespace ApiVersioning.Examples;

using Microsoft.Owin.Hosting;
using System.Diagnostics;

public class Program
{
    private const string Url = "http://localhost:9006/";
    private const string LaunchUrl = Url + "api";
    private static readonly ManualResetEvent resetEvent = new( false );

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