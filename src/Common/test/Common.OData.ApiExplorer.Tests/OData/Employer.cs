// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData;

public class Employer
{
    public int EmployerId { get; set; }

    public ICollection<Employee> Employees { get; set; }

    public DateTime Birthday { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }
}