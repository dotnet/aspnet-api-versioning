// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Grpc;

internal sealed class HttpRouteVariable
{
    public int StartSegment { get; set; }

    public int EndSegment { get; set; }

    public List<string> FieldPath { get; } = [];

    public bool HasCatchAllPath { get; set; }
}