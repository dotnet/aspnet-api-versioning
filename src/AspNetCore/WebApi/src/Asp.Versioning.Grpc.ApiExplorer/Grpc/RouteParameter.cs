// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Grpc;

using Google.Protobuf.Reflection;

internal sealed class RouteParameter(
    List<FieldDescriptor> descriptorsPath,
    HttpRouteVariable routeVariable,
    string jsonPath )
{
    public List<FieldDescriptor> DescriptorsPath { get; } = descriptorsPath;

    public HttpRouteVariable RouteVariable { get; } = routeVariable;

    public string JsonPath { get; } = jsonPath;
}