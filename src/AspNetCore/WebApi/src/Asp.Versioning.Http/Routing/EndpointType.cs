// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

internal enum EndpointType
{
    UserDefined,
    Malformed,
    Ambiguous,
    Unspecified,
    UnsupportedMediaType,
    AssumeDefault,
    NotAcceptable,
    Unsupported,
}