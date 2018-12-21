namespace Microsoft.Examples
{
    using Microsoft.Owin.Hosting;
    using System;
    using System.Threading;

    public class Program
    {
        const string Url = "http://localhost:9000/";
        static readonly ManualResetEvent resetEvent = new ManualResetEvent( false );

        public static void Main( string[] args )
        {
            Console.CancelKeyPress += OnCancel;

            using ( WebApp.Start<Startup>( Url ) )
            {
                Console.WriteLine( "Content root path: " + Startup.ContentRootPath );
                Console.WriteLine( "Now listening on: " + Url );
                Console.WriteLine( "Application started. Press Ctrl+C to shut down." );
                resetEvent.WaitOne();
            }

            Console.CancelKeyPress -= OnCancel;
        }

        static void OnCancel( object sender, ConsoleCancelEventArgs e )
        {
            Console.Write( "Application is shutting down..." );
            e.Cancel = true;
            resetEvent.Set();
        }
    }
}