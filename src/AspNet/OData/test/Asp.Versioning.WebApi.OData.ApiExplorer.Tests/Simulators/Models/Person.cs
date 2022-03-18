// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Simulators.Models;

using System.ComponentModel.DataAnnotations;

public class Person
{
    [Key]
    public int Id { get; set; }
}