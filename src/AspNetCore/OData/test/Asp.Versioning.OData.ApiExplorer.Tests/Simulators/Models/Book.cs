// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Simulators.Models;

using Microsoft.OData.ModelBuilder;

/// <summary>
/// Represents a book.
/// </summary>
[Filter( "author", "published" )]
public class Book
{
    /// <summary>
    /// Gets or sets the book identifier.
    /// </summary>
    /// <value>The International Standard Book Number (ISBN).</value>
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the book author.
    /// </summary>
    /// <value>The author of the book.</value>
    public string Author { get; set; }

    /// <summary>
    /// Gets or sets the book title.
    /// </summary>
    /// <value>The title of the book.</value>
    public string Title { get; set; }

    /// <summary>
    /// Gets or sets the book publication year.
    /// </summary>
    /// <value>The year the book was first published.</value>
    public int Published { get; set; }
}