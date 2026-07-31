// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OpenApi.Simulators;

/// <summary>
/// Represents an order.
/// </summary>
public class Order
{
    /// <summary>
    /// Gets or sets the order identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the customer.
    /// </summary>
    public string Customer { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the address the order ships to.
    /// </summary>
    public Address ShipTo { get; set; } = new();

    /// <summary>
    /// Gets or sets the order notes.
    /// </summary>
    [VisibleInApiVersion( "2.0" )]
    public string Notes { get; set; } = string.Empty;
}