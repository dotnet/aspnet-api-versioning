// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace Google.Protobuf.Reflection;

internal static class DescriptorBaseExtensions
{
    extension( DescriptorBase descriptor )
    {
        public bool IsWrapperType => descriptor.File.Package == "google.protobuf"
                                     && descriptor.File.Name == "google/protobuf/wrappers.proto";
    }
}