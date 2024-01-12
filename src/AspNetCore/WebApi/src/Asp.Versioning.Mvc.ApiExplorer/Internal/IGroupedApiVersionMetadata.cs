// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApiExplorer.Internal;

internal interface IGroupedApiVersionMetadata
{
    string? GroupName { get; }

    string Name { get; }

    bool IsApiVersionNeutral { get; }

    ApiVersionModel Map( ApiVersionMapping mapping );

    ApiVersionMapping MappingTo( ApiVersion? apiVersion );

    bool IsMappedTo( ApiVersion? apiVersion );

    void Deconstruct( out ApiVersionModel apiModel, out ApiVersionModel endpointModel );
}