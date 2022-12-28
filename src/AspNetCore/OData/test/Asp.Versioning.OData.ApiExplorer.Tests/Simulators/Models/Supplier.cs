// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Simulators.Models;

public class Supplier
{
    public int Id { get; set; }

    public string Name { get; set; }

#pragma warning disable CA2227 // Collection properties should be read only
    public ICollection<Product> Products { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only
}