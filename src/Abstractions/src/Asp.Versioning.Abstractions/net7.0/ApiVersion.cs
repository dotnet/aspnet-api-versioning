// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

/// <content>
/// Contains additional implementation specific to .NET 6.0.
/// </content>
public partial class ApiVersion : ISpanFormattable
{
    /// <inheritdoc />
    public virtual bool TryFormat( Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider )
    {
        var instance = ApiVersionFormatProvider.GetInstance( provider );
        return instance.TryFormat( destination, out charsWritten, format, this, provider );
    }
}