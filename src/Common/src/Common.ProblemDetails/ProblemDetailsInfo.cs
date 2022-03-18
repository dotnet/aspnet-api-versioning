// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

/// <summary>
/// Represents problem details information.
/// </summary>
public class ProblemDetailsInfo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProblemDetailsInfo"/> class.
    /// </summary>
    /// <param name="type">The problem details type.</param>
    /// <param name="title">The problem details title.</param>
    /// <param name="code">The optional problem details error code.</param>
    public ProblemDetailsInfo( string type, string title, string? code = default )
    {
        Type = type;
        Title = title;
        Code = code;
    }

    /// <summary>
    /// Gets the problem details type.
    /// </summary>
    /// <value>The problem details type.</value>
    public string Type { get; }

    /// <summary>
    /// Gets the problem details title.
    /// </summary>
    /// <value>The problem details title.</value>
    public string Title { get; }

    /// <summary>
    /// Gets the problem details error code.
    /// </summary>
    /// <value>The problem details error code.</value>
    public string? Code { get; }

    /// <summary>
    /// Deconstructs the information into its constituent parts.
    /// </summary>
    /// <param name="type">The problem details type.</param>
    /// <param name="title">The problem details title.</param>
    public void Deconstruct( out string type, out string title )
    {
        title = Title;
        type = Type;
    }

    /// <summary>
    /// Deconstructs the information into its constituent parts.
    /// </summary>
    /// <param name="type">The problem details type.</param>
    /// <param name="title">The problem details title.</param>
    /// <param name="code">The problem details error code.</param>
    public void Deconstruct( out string type, out string title, out string? code )
    {
        title = Title;
        type = Type;
        code = Code;
    }
}