// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

#if NET
using System.Text;
#endif

internal static class Format
{
#if NETSTANDARD
    internal static readonly string ApiVersionBadStatus = SR.ApiVersionBadStatus;
    internal static readonly string ApiVersionBadGroupVersion = SR.ApiVersionBadGroupVersion;
    internal static readonly string InvalidRelationType = SR.InvalidRelationType;
#else
    internal static readonly CompositeFormat ApiVersionBadStatus = CompositeFormat.Parse( SR.ApiVersionBadStatus );
    internal static readonly CompositeFormat ApiVersionBadGroupVersion = CompositeFormat.Parse( SR.ApiVersionBadGroupVersion );
    internal static readonly CompositeFormat InvalidRelationType = CompositeFormat.Parse( SR.InvalidRelationType );
#endif
}