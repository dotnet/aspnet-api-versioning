// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Grpc;

using Google.Protobuf.Reflection;
using System.Reflection;

internal sealed class BodyDescriptorInfo
{
    public required MessageDescriptor Descriptor { get; init; }

    public required FieldDescriptor? FieldDescriptor { get; init; }

    public required bool IsDescriptorRepeated { get; init; }

    public required PropertyInfo? PropertyInfo { get; init; }

    public required ParameterInfo? ParameterInfo { get; init; }
}