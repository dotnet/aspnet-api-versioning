namespace Microsoft.Web
{
    using System;
    using System.Net.Http;
    using System.Web.Http.Tracing;
    using static System.Diagnostics.Debug;

    public sealed class TraceWriter : ITraceWriter
    {
        TraceWriter() { }

        public static ITraceWriter None { get; } = new TraceWriter();

        public static ITraceWriter Debug { get; } = new DebugTraceWriter();

        public void Trace( HttpRequestMessage request, string category, TraceLevel level, Action<TraceRecord> traceAction ) { }

        sealed class DebugTraceWriter : ITraceWriter
        {
            public void Trace( HttpRequestMessage request, string category, TraceLevel level, Action<TraceRecord> traceAction )
            {
                var record = new TraceRecord( request, category, level );
                traceAction?.Invoke( record );

                WriteLine( $"[{nameof( record.Category )}={record.Category},{nameof( record.Operator )}={record.Operator},{nameof( record.Operation )}={record.Operation}] {record.Message}" );

                if ( record.Exception != null )
                {
                    WriteLine( record.Exception );
                }
            }
        }
    }
}