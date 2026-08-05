// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Analyzers;

/// <remarks>
/// The template of an endpoint, along with whether every part of it was resolved. A template that
/// could not be followed to its origin may be missing a prefix that carries the constraint, so it can
/// only be trusted when the constraint was already found in the part that was resolved.
/// </remarks>
internal readonly struct Route( string template, bool complete )
{
    public string Template { get; } = template;

    public bool Complete { get; } = complete;
}