// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Analyzers;

/// <remarks>
/// The templates an endpoint answers to, and whether it declares an API version of its own. An
/// endpoint answering to both a constrained and an unconstrained template is registered twice on
/// purpose, which is the one way a default version can apply to a URL segment.
/// </remarks>
internal readonly struct Endpoint(
    IReadOnlyList<string> templates,
    bool versioned,
    bool neutral,
    string? @namespace = default )
{
    public IReadOnlyList<string> Templates { get; } = templates;

    /// <summary>
    /// Gets a value indicating whether the endpoint declares an explicit API version.
    /// </summary>
    public bool Versioned { get; } = versioned;

    /// <summary>
    /// Gets a value indicating whether the endpoint declares that it is version-neutral.
    /// </summary>
    /// <remarks>Neutrality is metadata in its own right, so it takes an endpoint out of the
    /// arrangement a default version is meant for without giving it a version of its own.</remarks>
    public bool Neutral { get; } = neutral;

    /// <summary>
    /// Gets a value indicating whether the endpoint declares any versioning metadata at all.
    /// </summary>
    public bool Declared => Versioned || Neutral;

    /// <summary>
    /// Gets the namespace declaring the endpoint, if it came from a controller.
    /// </summary>
    /// <remarks>Whether a namespace confers a version depends on which conventions are registered,
    /// which is not known until the compilation has been seen in full.</remarks>
    public string? Namespace { get; } = @namespace;
}