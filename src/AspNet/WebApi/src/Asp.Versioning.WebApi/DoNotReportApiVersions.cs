// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using static Asp.Versioning.ApiVersionMapping;

internal sealed class DoNotReportApiVersions : IReportApiVersions
{
    public ApiVersionMapping Mapping => Explicit | Implicit;

    public void Report( HttpResponseMessage response, ApiVersionModel apiVersionModel ) { }
}