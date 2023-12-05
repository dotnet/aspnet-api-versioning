// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using System.Text;

internal static class Format
{
    internal static readonly CompositeFormat MultipleDifferentApiVersionsRequested = CompositeFormat.Parse( CommonSR.MultipleDifferentApiVersionsRequested );
    internal static readonly CompositeFormat NoVersionSet = CompositeFormat.Parse( SR.NoVersionSet );
    internal static readonly CompositeFormat InvalidMediaTypeTemplate = CompositeFormat.Parse( CommonSR.InvalidMediaTypeTemplate );
    internal static readonly CompositeFormat UnsetRequestDelegate = CompositeFormat.Parse( SR.UnsetRequestDelegate );
    internal static readonly CompositeFormat VersionedResourceNotSupported = CompositeFormat.Parse( SR.VersionedResourceNotSupported );
    internal static readonly CompositeFormat InvalidDefaultApiVersion = CompositeFormat.Parse( SR.InvalidDefaultApiVersion );
    internal static readonly CompositeFormat InvalidPolicyKey = CompositeFormat.Parse( CommonSR.InvalidPolicyKey );
}