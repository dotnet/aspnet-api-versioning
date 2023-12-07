// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0079
#pragma warning disable CA2227 // Collection properties should be read only

namespace Asp.Versioning.OData;

public class Employer
{
    public int EmployerId { get; set; }

    public ICollection<Employee> Employees { get; set; }

    public DateTime Birthday { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }
}