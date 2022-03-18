// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData;

public class Shipment
{
    public int Id { get; set; }

    public DateTime ShippedOn { get; set; }

    public double Weight { get; set; }

    public Address Destination { get; set; }
}