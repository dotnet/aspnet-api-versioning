// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace System;

using static System.Globalization.CultureInfo;

internal static class DateTimeOffsetExtensions
{
    extension( DateTimeOffset dateTime )
    {
        public string ToDeprecationHeaderValue() => dateTime.ToUnixTimeSeconds().ToString( "'@'0", InvariantCulture );
    }
}