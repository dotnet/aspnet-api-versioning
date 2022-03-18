// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using System.Collections;

public abstract class SelectVersionData : IEnumerable<object[]>
{
    public abstract IEnumerator<object[]> GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    protected static IEnumerable<ApiVersion> Supported( params ApiVersion[] versions ) => versions.AsEnumerable();

    protected static IEnumerable<ApiVersion> Deprecated( params ApiVersion[] versions ) => versions.AsEnumerable();

    protected static ApiVersion Expected( ApiVersion version ) => version;
}