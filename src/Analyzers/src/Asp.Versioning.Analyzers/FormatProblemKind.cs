// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Analyzers;

internal enum FormatProblemKind
{
    /// <summary>A quoted literal is never closed, which throws when the format is applied.</summary>
    UnterminatedLiteral,

    /// <summary>A padding count is not a number or exceeds the supported maximum.</summary>
    PaddingOutOfRange,

    /// <summary>A specifier is repeated more times than is meaningful.</summary>
    RepeatedSpecifier,
}