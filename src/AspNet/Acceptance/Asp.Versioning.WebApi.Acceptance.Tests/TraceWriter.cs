// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using System.Net.Http;
using System.Web.Http.Tracing;
using static System.Diagnostics.Debug;

internal sealed class TraceWriter : ITraceWriter
{
    private TraceWriter() { }

    public static ITraceWriter None { get; } = new TraceWriter();

    public static ITraceWriter Debug { get; } = new DebugTraceWriter();

    public void Trace( HttpRequestMessage request, string category, TraceLevel level, Action<TraceRecord> traceAction ) { }

    private sealed class DebugTraceWriter : ITraceWriter
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