// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

public sealed class MinSelectVersionData : SelectVersionData
{
    public MinSelectVersionData()
    {
        Add(
            Supported( new ApiVersion( 1, 0 ), new ApiVersion( 2, 0 ), new ApiVersion( 3, 0, "Alpha" ) ),
            Deprecated(),
            Expected( new ApiVersion( 1, 0 ) ) );

        Add(
            Supported( new ApiVersion( 0, 9, "RC" ), new ApiVersion( 1, 0 ), new ApiVersion( 2, 0 ) ),
            Deprecated( new ApiVersion( 3, 0 ) ),
            Expected( new ApiVersion( 1, 0 ) ) );

        Add(
            Supported( new ApiVersion( 2, 0 ), new ApiVersion( 3, 1, "Beta" ) ),
            Deprecated( new ApiVersion( 1, 0 ), new ApiVersion( 3, 0 ) ),
            Expected( new ApiVersion( 1, 0 ) ) );

        Add(
            Supported(),
            Deprecated(),
            Expected( new ApiVersion( 42, 0 ) ) );

        Add(
            Supported( new ApiVersion( 1, 1, "RC1" ) ),
            Deprecated(),
            Expected( new ApiVersion( 42, 0 ) ) );

        Add(
            Supported( new ApiVersion( 2, 5 ) ),
            Deprecated(),
            Expected( new ApiVersion( 2, 5 ) ) );

        Add(
            Supported( new ApiVersion( 0, 8, "Beta" ), new ApiVersion( 0, 9, "RC" ) ),
            Deprecated(),
            Expected( new ApiVersion( 42, 0 ) ) );
    }
}