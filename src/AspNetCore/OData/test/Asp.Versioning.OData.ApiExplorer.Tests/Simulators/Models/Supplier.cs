// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Simulators.Models;

public class Supplier
{
    public int Id { get; set; }

    public string Name { get; set; }

    public ICollection<Product> Products { get; set; }
}