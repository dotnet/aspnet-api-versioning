// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

internal sealed class ApiVersionPolicyFeature
{
    public ApiVersionPolicyFeature( ApiVersionMetadata metadata ) => Metadata = metadata;

    public ApiVersionMetadata Metadata { get; }
}