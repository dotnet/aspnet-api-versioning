// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable CA2227 // Collection properties should be read only

namespace Asp.Versioning.Simulators.Models;

public class Supplier
{
    public int Id { get; set; }

    public string Name { get; set; }

    public ICollection<Product> Products { get; set; }
}