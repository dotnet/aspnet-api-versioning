// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Simulators.Models;

using Microsoft.AspNet.OData.Query;

[Filter( "author", "published" )]
public class Book
{
    public string Id { get; set; }

    public string Author { get; set; }

    public string Title { get; set; }

    public int Published { get; set; }
}