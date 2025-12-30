// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

public abstract class SelectVersionData : TheoryData<ApiVersion[], ApiVersion[], ApiVersion>
{
    protected SelectVersionData() { }

    protected static ApiVersion[] Supported( params ApiVersion[] versions ) => versions;

    protected static ApiVersion[] Deprecated( params ApiVersion[] versions ) => versions;

    protected static ApiVersion Expected( ApiVersion version ) => version;
}