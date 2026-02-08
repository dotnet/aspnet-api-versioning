// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace System;

internal static class NullableExtensions
{
    extension( int? value )
    {
        public bool Unset => value.HasValue && value.Value == 0;

        public bool NoLimitOrSome => !value.HasValue || value.Value > 0;

        public bool NoLimitOrNone => !value.HasValue || value.Value <= 0;
    }
}