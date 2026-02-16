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
    /// Get ACME Supplier
    /// </summary>
    /// <description>Retrieves the ACME supplier.</description>
    /// <returns>All available suppliers.</returns>
    /// <response code="200">The supplier was successfully retrieved.</response>
    [HttpGet]
    [EnableQuery]
    [Produces( "application/json" )]
    [ProducesResponseType( typeof( ODataValue<Supplier> ), Status200OK )]
    public IActionResult Get() => Ok( NewSupplier() );

    /// <summary>
    /// Get Products
    /// </summary>
    /// <description>Gets the products associated with the supplier.</description>
    /// <returns>The associated supplier products.</returns>
    /// <response code="200">The products were successfully retrieved.</response>
    [HttpGet]
    [EnableQuery]
    [Produces( "application/json" )]
    [ProducesResponseType( typeof( ODataValue<IEnumerable<Product>> ), Status200OK )]
    public IQueryable<Product> GetProducts() => NewSupplier().Products.AsQueryable();

    /// <summary>
    /// Link Product
    /// </summary>
    /// <description>Links a product to a supplier.</description>
    /// <param name="navigationProperty">The name of the related navigation property.</param>
    /// <param name="link">The related entity identifier.</param>
    /// <returns>None</returns>
    /// <response code="204">The product was successfully linked.</response>
    [HttpPut]
    [ProducesResponseType( Status204NoContent )]
    public IActionResult CreateRef( string navigationProperty, [FromBody] Uri link )
    {
        var relatedKey = this.GetRelatedKey( link );
        return NoContent();
    }

    /// <summary>
    /// Unlink Product
    /// </summary>
    /// <description>Unlinks a product from a supplier.</description>
    /// <param name="relatedKey">The related entity identifier.</param>
    /// <param name="navigationProperty">The name of the related navigation property.</param>
    /// <returns>None</returns>
    /// <response code="204">The product was successfully unlinked.</response>
    [HttpDelete]
    [ProducesResponseType( Status204NoContent )]
    public IActionResult DeleteRef( int relatedKey, string navigationProperty ) => NoContent();

    private static Supplier NewSupplier() => new()
    {
        Id = 42,
        Name = "Acme",
        Products =
        [
            new()
            {
                Id = 42,
                Name = "Product 42",
                Category = "Test",
                Price = 42,
                SupplierId = 42,
            },
        ],
    };
}