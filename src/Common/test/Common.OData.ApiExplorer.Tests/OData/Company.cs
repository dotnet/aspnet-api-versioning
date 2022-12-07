// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData;

public class Company
{
    public int CompanyId { get; set; }

    public Company ParentCompany { get; set; }

    public List<Company> Subsidiaries { get; set; }

    public string Name { get; set; }

    public DateTime DateFounded { get; set; }
}