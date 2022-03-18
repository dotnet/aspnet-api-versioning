// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0079
#pragma warning disable SA1121

namespace Asp.Versioning;

using System.Diagnostics;
#if NETSTANDARD1_0
using Text = System.String;
#else
using Text = System.ReadOnlySpan<char>;
#endif

[DebuggerDisplay( $"{nameof( Format )} = {{{nameof( Format )},nq}}, {nameof( IsLiteral )} = {{{nameof( IsLiteral )},nq}}" )]
internal readonly ref struct FormatToken
{
    public readonly Text Format;
    public readonly bool IsLiteral;

    internal FormatToken( Text format, bool literal = false )
    {
        Format = format;
        IsLiteral = literal;
    }
}