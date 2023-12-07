// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0079
#pragma warning disable CA1002 // Do not expose generic lists
#pragma warning disable CA2227 // Collection properties should be read only

namespace Asp.Versioning.OData;

public class Company
{
    public int CompanyId { get; set; }

    public Company ParentCompany { get; set; }

    public List<Company> Subsidiaries { get; set; }

    public string Name { get; set; }

    public DateTime DateFounded { get; set; }
}