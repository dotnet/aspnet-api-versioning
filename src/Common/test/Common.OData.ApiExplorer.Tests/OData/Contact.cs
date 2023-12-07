// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0079
#pragma warning disable CA1002 // Do not expose generic lists
#pragma warning disable CA2227 // Collection properties should be read only

namespace Asp.Versioning.OData;

public class Contact
{
    public int ContactId { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string Email { get; set; }

    public string Phone { get; set; }

    public List<Address> Addresses { get; set; }
}