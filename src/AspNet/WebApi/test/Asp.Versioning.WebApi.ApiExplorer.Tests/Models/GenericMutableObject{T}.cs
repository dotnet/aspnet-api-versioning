// Copyright (c) .NET Foundation and contributors. All rights reserved.

//// Ignore Spelling: Foo

namespace Asp.Versioning.Models;

public class GenericMutableObject<T> : List<T>
{
    public string Foo { get; set; }

    public string Bar { get; set; }
}