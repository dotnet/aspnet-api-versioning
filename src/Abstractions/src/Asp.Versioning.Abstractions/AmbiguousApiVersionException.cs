// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

/// <summary>
/// Represents the exception thrown when multiple, different API versions specified in a single request.
/// </summary>
public partial class AmbiguousApiVersionException : Exception
{
    private readonly string[] apiVersions;

    /// <summary>
    /// Initializes a new instance of the <see cref="AmbiguousApiVersionException"/> class.
    /// </summary>
    public AmbiguousApiVersionException() => apiVersions = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="AmbiguousApiVersionException"/> class.
    /// </summary>
    /// <param name="message">The associated error message.</param>
    public AmbiguousApiVersionException( string message )
        : base( message ) => apiVersions = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="AmbiguousApiVersionException"/> class.
    /// </summary>
    /// <param name="message">The associated error message.</param>
    /// <param name="innerException">The inner <see cref="Exception">exception</see> that caused the current exception, if any.</param>
    public AmbiguousApiVersionException( string message, Exception innerException )
        : base( message, innerException ) => apiVersions = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="AmbiguousApiVersionException"/> class.
    /// </summary>
    /// <param name="message">The associated error message.</param>
    /// <param name="apiVersions">The <see cref="IEnumerable{T}">sequence</see> of ambiguous API versions.</param>
    public AmbiguousApiVersionException( string message, IEnumerable<string> apiVersions )
        : base( message ) => this.apiVersions = apiVersions.ToArray();

    /// <summary>
    /// Initializes a new instance of the <see cref="AmbiguousApiVersionException"/> class.
    /// </summary>
    /// <param name="message">The associated error message.</param>
    /// <param name="apiVersions">The <see cref="IEnumerable{T}">sequence</see> of ambiguous API versions.</param>
    /// <param name="innerException">The inner <see cref="Exception">exception</see> that caused the current exception, if any.</param>
    public AmbiguousApiVersionException( string message, IEnumerable<string> apiVersions, Exception innerException )
        : base( message, innerException ) => this.apiVersions = apiVersions.ToArray();

    /// <summary>
    /// Gets a read-only list of the ambiguous API versions.
    /// </summary>
    /// <value>A <see cref="IReadOnlyList{T}">read-only list</see> of unparsed, ambiguous API versions.</value>
    public IReadOnlyList<string> ApiVersions => apiVersions;
}