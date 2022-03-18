// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData.Models;

using System.ComponentModel.DataAnnotations;

public class Person
{
    public int Id { get; set; }

    [Required]
    [StringLength( 25 )]
    public string FirstName { get; set; }

    [Required]
    [StringLength( 25 )]
    public string LastName { get; set; }

    public string Email { get; set; }

    public string Phone { get; set; }
}