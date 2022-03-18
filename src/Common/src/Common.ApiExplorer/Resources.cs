// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

internal static class Resources
{
#if NETFRAMEWORK
    public const string BaseName = "Asp.Versioning.ExpSR";
#else
    public const string BaseName = "Asp.Versioning.ApiExplorer.ExpSR";
#endif
}