namespace ApiVersioning.Examples.Models;

using Microsoft.OData.ModelBuilder;

/// <summary>
/// Represents the line item on an order.
/// </summary>
[Select]
public class LineItem
{
    /// <summary>
    /// Gets or sets the line item number.
    /// </summary>
    /// <value>The line item number.</value>
    public int Number { get; set; }

    /// <summary>
    /// Gets or sets the line item description.
    /// </summary>
    /// <value>The line item description.</value>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the line item quantity.
    /// </summary>
    /// <value>The line item quantity.</value>
    public int Quantity { get; set; }

    /// <summary>
    /// Gets or sets the line item unit price.
    /// </summary>
    /// <value>The line item unit price.</value>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the line item is fulfilled.
    /// </summary>
    /// <value>True if the line item is fulfilled; otherwise, false.</value>
    public bool Fulfilled { get; set; }
}