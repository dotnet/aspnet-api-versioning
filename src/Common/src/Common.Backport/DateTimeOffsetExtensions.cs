// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace System;

internal static class DateTimeOffsetExtensions
{
    private const long UnixEpochSeconds = 62_135_596_800L;

    // REF: https://github.com/dotnet/dotnet/blob/main/src/runtime/src/libraries/System.Private.CoreLib/src/System/DateTimeOffset.cs#L745
    public static long ToUnixTimeSeconds( this DateTimeOffset dateTimeOffset )
    {
        var seconds = (long) ( (ulong) dateTimeOffset.UtcTicks / TimeSpan.TicksPerSecond );
        return seconds - UnixEpochSeconds;
    }
}