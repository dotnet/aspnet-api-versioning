namespace Microsoft.Web
{
    using System;
    using System.Net.Http;
    using System.Web.Http.Tracing;

    public sealed class TraceWriter : ITraceWriter
    {
        public void Trace( HttpRequestMessage request, string category, TraceLevel level, Action<TraceRecord> traceAction ) { }
    }
}