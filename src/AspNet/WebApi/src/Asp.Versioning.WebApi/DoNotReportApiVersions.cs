// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using static Asp.Versioning.ApiVersionMapping;

internal sealed class DoNotReportApiVersions : IReportApiVersions
{
    private static DoNotReportApiVersions? instance;

    private DoNotReportApiVersions() { }

    internal static IReportApiVersions Instance => instance ??= new();

    public ApiVersionMapping Mapping { get; } = Explicit | Implicit;

    public void Report( HttpResponseMessage response, ApiVersionModel apiVersionModel ) { }
}