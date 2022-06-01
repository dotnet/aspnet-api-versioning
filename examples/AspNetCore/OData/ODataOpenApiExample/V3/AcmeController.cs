namespace ApiVersioning.Examples.V3;

using ApiVersioning.Examples.Models;
using Asp.Versioning;
using Asp.Versioning.OData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using static Microsoft.AspNetCore.Http.StatusCodes;

/// <summary>
/// Represents a RESTful service for the ACME supplier.
/// </summary>
[ApiVersion( 3.0 )]
public class AcmeController : ODataController
{
    /// <summary>
    /// Retrieves the ACME supplier.
    /// </summary>
    /// <returns>All available suppliers.</returns>
    /// <response code="200">The supplier successfully retrieved.</response>
    [HttpGet]
    [EnableQuery]
    [Produces( "application/json" )]
    [ProducesResponseType( typeof( ODataValue<Supplier> ), Status200OK )]
    public IActionResult Get() => Ok( NewSupplier() );

    /// <summary>
    /// Gets the products associated with the supplier.
    /// </summary>
    /// <returns>The associated supplier products.</returns>
    [HttpGet]
    [EnableQuery]
    public IQueryable<Product> GetProducts() => NewSupplier().Products.AsQueryable();

    /// <summary>
    /// Links a product to a supplier.
    /// </summary>
    /// <param name="navigationProperty">The name of the related navigation property.</param>
    /// <param name="link">The related entity identifier.</param>
    /// <returns>None</returns>
    [HttpPut]
    [ProducesResponseType( Status204NoContent )]
    [ProducesResponseType( Status404NotFound )]
    public IActionResult CreateRef(
        string navigationProperty,
        [FromBody] Uri link )
    {
        var relatedKey = this.GetRelatedKey( link );
        return NoContent();
    }

    /// <summary>
    /// Unlinks a product from a supplier.
    /// </summary>
    /// <param name="relatedKey">The related entity identifier.</param>
    /// <param name="navigationProperty">The name of the related navigation property.</param>
    /// <returns>None</returns>
    [HttpDelete]
    [ProducesResponseType( Status204NoContent )]
    [ProducesResponseType( Status404NotFound )]
    public IActionResult DeleteRef(
        int relatedKey,
        string navigationProperty ) => NoContent();

    private static Supplier NewSupplier() =>
        new()
        {
            Id = 42,
            Name = "Acme",
            Products = new List<Product>()
            {
                new()
                {
                    Id = 42,
                    Name = "Product 42",
                    Category = "Test",
                    Price = 42,
                    SupplierId = 42,
                },
            },
        };
}