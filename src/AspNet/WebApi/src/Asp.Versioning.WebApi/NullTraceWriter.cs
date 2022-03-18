// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using System.Web.Http.Tracing;

internal sealed class NullTraceWriter : ITraceWriter
{
    private static NullTraceWriter? instance;

    private NullTraceWriter() { }

    internal static ITraceWriter Instance => instance ??= new();

    public void Trace( HttpRequestMessage request, string category, TraceLevel level, Action<TraceRecord> traceAction ) { }
}