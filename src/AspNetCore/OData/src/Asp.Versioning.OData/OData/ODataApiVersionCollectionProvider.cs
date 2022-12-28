// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0079
#pragma warning disable CA1812

namespace Asp.Versioning.OData;

internal sealed class ODataApiVersionCollectionProvider : IODataApiVersionCollectionProvider
{
    private IReadOnlyList<ApiVersion>? apiVersions;

    public IReadOnlyList<ApiVersion> ApiVersions
    {
        get => apiVersions ?? Array.Empty<ApiVersion>();
        set => apiVersions = value;
    }
}