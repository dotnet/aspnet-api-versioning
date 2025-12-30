// Copyright (c) .NET Foundation and contributors. All rights reserved.

using Asp.Versioning;

[assembly: RegisterXunitSerializer( typeof( TestSerializer ), typeof( ApiVersion ) )]

namespace Asp.Versioning;

public sealed class TestSerializer : XunitSerializer<ApiVersion>
{
    public override string Serialize( ApiVersion value ) => value?.ToString() ?? string.Empty;

    public override ApiVersion Deserialize( Type type, string serializedValue ) =>
        ApiVersionParser.Default.Parse( serializedValue );
}