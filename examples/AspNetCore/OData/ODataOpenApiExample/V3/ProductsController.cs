namespace ApiVersioning.Examples.V3;

using ApiVersioning.Examples.Models;
using Asp.Versioning;
using Asp.Versioning.OData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.OData.UriParser;
using static Microsoft.AspNetCore.Http.StatusCodes;

/// <summary>
/// Represents a RESTful service of products.
/// </summary>
[ApiVersion( 3.0 )]
public class ProductsController : ODataController
{
    private readonly IQueryable<Product> products = new[]
    {
        NewProduct( 1 ),
        NewProduct( 2 ),
        NewProduct( 3 ),
    }.AsQueryable();

    /// <summary>
    /// Get Products
    /// </summary>
    /// <description>Retrieves all products.</description>
    /// <returns>All available products.</returns>
    /// <response code="200">Products successfully retrieved.</response>
    [HttpGet]
    [EnableQuery]
    [Produces( "application/json" )]
    [ProducesResponseType( typeof( ODataValue<IEnumerable<Product>> ), Status200OK )]
    public IQueryable<Product> Get() => products;

    /// <summary>
    /// Get Product
    /// </summary>
    /// <description>Gets a single product.</description>
    /// <param name="key">The requested product identifier.</param>
    /// <returns>The requested product.</returns>
    /// <response code="200">The product was successfully retrieved.</response>
    /// <response code="404">The product does not exist.</response>
    [HttpGet]
    [EnableQuery]
    [Produces( "application/json" )]
    [ProducesResponseType( typeof( Product ), Status200OK )]
    [ProducesResponseType( Status404NotFound )]
    public SingleResult<Product> Get( int key ) =>
        SingleResult.Create( products.Where( p => p.Id == key ) );

    /// <summary>
    /// Add Product
    /// </summary>
    /// <description>Adds a new product.</description>
    /// <param name="product">The product to add.</param>
    /// <returns>The added product.</returns>
    /// <response code="201">The product was successfully added.</response>
    /// <response code="204">The product was successfully added.</response>
    /// <response code="400">The product is invalid.</response>
    [HttpPost]
    [Produces( "application/json" )]
    [ProducesResponseType( typeof( Product ), Status201Created )]
    [ProducesResponseType( Status204NoContent )]
    [ProducesResponseType( Status400BadRequest )]
    public IActionResult Post( [FromBody] Product product )
    {
        if ( !ModelState.IsValid )
        {
            return BadRequest( ModelState );
        }

        product.Id = 42;

        return Created( product );
    }

    /// <summary>
    /// Update Product (Partial)
    /// </summary>
    /// <description>Updates an existing product.</description>
    /// <param name="key">The requested product identifier.</param>
    /// <param name="delta">The partial product to update.</param>
    /// <returns>The updated product.</returns>
    /// <response code="200">The product was successfully updated.</response>
    /// <response code="204">The product was successfully updated.</response>
    /// <response code="400">The product is invalid.</response>
    /// <response code="404">The product does not exist.</response>
    [HttpPatch]
    [Produces( "application/json" )]
    [ProducesResponseType( typeof( Product ), Status200OK )]
    [ProducesResponseType( Status204NoContent )]
    [ProducesResponseType( Status400BadRequest )]
    [ProducesResponseType( Status404NotFound )]
    public IActionResult Patch( int key, [FromBody] Delta<Product> delta )
    {
        if ( !ModelState.IsValid )
        {
            return BadRequest( ModelState );
        }

        var product = new Product() { Id = key, Name = "Updated Product " + key.ToString() };

        delta.Patch( product );

        return Updated( delta );
    }

    /// <summary>
    /// Update Product
    /// </summary>
    /// <description>Updates an existing product.</description>
    /// <param name="key">The requested product identifier.</param>
    /// <param name="update">The product to update.</param>
    /// <returns>The updated product.</returns>
    /// <response code="200">The product was successfully updated.</response>
    /// <response code="204">The product was successfully updated.</response>
    /// <response code="400">The product is invalid.</response>
    /// <response code="404">The product does not exist.</response>
    [HttpPut]
    [Produces( "application/json" )]
    [ProducesResponseType( typeof( Product ), Status200OK )]
    [ProducesResponseType( Status204NoContent )]
    [ProducesResponseType( Status400BadRequest )]
    [ProducesResponseType( Status404NotFound )]
    public IActionResult Put( int key, [FromBody] Product update )
    {
        if ( !ModelState.IsValid )
        {
            return BadRequest( ModelState );
        }

        return Updated( update );
    }

    /// <summary>
    /// Remove Product
    /// </summary>
    /// <description>Removes a product.</description>
    /// <param name="key">The product to remove.</param>
    /// <returns>None</returns>
    /// <response code="204">The product was successfully removed.</response>
    [HttpDelete]
    [ProducesResponseType( Status204NoContent )]
    public IActionResult Delete( int key ) => NoContent();

    /// <summary>
    /// Get Supplier
    /// </summary>
    /// <description>Gets the supplier associated with the product.</description>
    /// <param name="key">The product identifier.</param>
    /// <returns>The requested supplier.</returns>
    /// <response code="200">The supplier was successfully retrieved.</response>
    /// <response code="404">The supplier was not found.</response>
    [HttpGet]
    [EnableQuery]
    [Produces( "application/json" )]
    [ProducesResponseType( typeof( Supplier ), Status200OK )]
    [ProducesResponseType( Status404NotFound )]
    public SingleResult<Supplier> GetSupplier( int key ) =>
        SingleResult.Create( products.Where( p => p.Id == key ).Select( p => p.Supplier ) );

    /// <summary>
    /// Get Supplier Reference
    /// </summary>
    /// <description>Gets the reference link to the associated supplier, if any.</description>
    /// <param name="key">The product identifier.</param>
    /// <param name="navigationProperty">The name of the related navigation property.</param>
    /// <returns>The supplier link.</returns>
    /// <response code="200">The supplier reference was successfully retrieved.</response>
    /// <response code="404">A supplier reference was not found.</response>
    [HttpGet]
    [Produces( "application/json" )]
    [ProducesResponseType( typeof( ODataId ), Status200OK )]
    [ProducesResponseType( Status404NotFound )]
    public IActionResult GetRef( int key, string navigationProperty )
    {
        var segments = Request.ODataFeature().Path.ToArray();
        var entitySet = ( (EntitySetSegment) segments[0] ).EntitySet;
        var property = entitySet.NavigationPropertyBindings.Single( p => p.Path.Path == navigationProperty ).NavigationProperty;

        segments[^1] = new NavigationPropertySegment( property, entitySet );

        var relatedKey = new Uri( Request.CreateODataLink( segments ) );

        return Ok( relatedKey );
    }

    /// <summary>
    /// Link Supplier
    /// </summary>
    /// <description>Links a supplier to a product.</description>
    /// <param name="key">The product identifier.</param>
    /// <param name="navigationProperty">The name of the related navigation property.</param>
    /// <param name="link">The related entity identifier.</param>
    /// <returns>None</returns>
    /// <response code="200">The supplier was successfully linked.</response>
    /// <response code="404">The product or supplier was not found.</response>
    [HttpPut]
    [ProducesResponseType( Status204NoContent )]
    [ProducesResponseType( Status404NotFound )]
    public IActionResult CreateRef( int key, string navigationProperty, [FromBody] Uri link )
    {
        var relatedKey = this.GetRelatedKey( link );
        return NoContent();
    }

    /// <summary>
    /// Unlink Supplier
    /// </summary>
    /// <description>Unlinks a supplier from a product.</description>
    /// <param name="key">The product identifier.</param>
    /// <param name="navigationProperty">The name of the related navigation property.</param>
    /// <param name="relatedKey">The related entity identifier.</param>
    /// <returns>None</returns>
    /// <response code="200">The supplier was successfully linked.</response>
    /// <response code="404">The product or supplier was not found.</response>
    [HttpDelete]
    [ProducesResponseType( Status204NoContent )]
    [ProducesResponseType( Status404NotFound )]
    public IActionResult DeleteRef( int key, string navigationProperty, int relatedKey ) => NoContent();

    private static Product NewProduct( int id ) => new()
    {
        Id = id,
        Category = "Test",
        Name = "Product " + id.ToString(),
        Price = id,
        Supplier = new Supplier() { Id = id, Name = "Supplier " + id.ToString() },
        SupplierId = id,
    };
}