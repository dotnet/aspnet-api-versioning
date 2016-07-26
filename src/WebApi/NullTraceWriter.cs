namespace Microsoft.Web.Http
{
    using System;
    using System.Net.Http;
    using System.Web.Http.Tracing;

    internal sealed class NullTraceWriter : ITraceWriter
    {
        private NullTraceWriter()
        {
        }

        internal static ITraceWriter Instance { get; } = new NullTraceWriter();

        public void Trace( HttpRequestMessage request, string category, TraceLevel level, Action<TraceRecord> traceAction )
        {
        }
    }
}
