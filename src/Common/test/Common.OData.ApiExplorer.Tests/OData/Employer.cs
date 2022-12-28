// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData;

public class Employer
{
    public int EmployerId { get; set; }

#pragma warning disable CA2227 // Collection properties should be read only
    public ICollection<Employee> Employees { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

    public DateTime Birthday { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }
}