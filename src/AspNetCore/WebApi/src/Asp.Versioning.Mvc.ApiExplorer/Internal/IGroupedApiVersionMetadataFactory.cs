// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApiExplorer.Internal;

internal interface IGroupedApiVersionMetadataFactory<out T>
    where T : IGroupedApiVersionMetadata
{
    static abstract T New( string? groupName, ApiVersionMetadata metadata );
}