// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData;

public class Company
{
    public int CompanyId { get; set; }

    public Company ParentCompany { get; set; }

#pragma warning disable CA1002 // Do not expose generic lists
#pragma warning disable CA2227 // Collection properties should be read only
    public List<Company> Subsidiaries { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only
#pragma warning restore CA1002 // Do not expose generic lists

    public string Name { get; set; }

    public DateTime DateFounded { get; set; }
}