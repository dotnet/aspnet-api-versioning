// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0060 // Remove unused parameter

namespace Asp.Versioning.Simulators.V3;

using Asp.Versioning.OData;
using Asp.Versioning.Simulators.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using static Microsoft.AspNetCore.Http.StatusCodes;

/// <summary>
/// Represents a RESTful service of suppliers.
/// </summary>
[ApiVersion( 3.0 )]
public class SuppliersController : ODataController
{
    private readonly IQueryable<Supplier> suppliers = new[]
    {
        NewSupplier( 1 ),
        NewSupplier( 2 ),
        NewSupplier( 3 ),
    }.AsQueryable();

    /// <summary>
    /// Retrieves all suppliers.
    /// </summary>
    /// <returns>All available suppliers.</returns>
    /// <response code="200">Products successfully retrieved.</response>
    [EnableQuery]
    [Produces( "application/json" )]
    [ProducesResponseType( typeof( ODataValue<IEnumerable<Supplier>> ), Status200OK )]
    public IQueryable<Supplier> Get() => suppliers;

    /// <summary>
    /// Gets a single supplier.
    /// </summary>
    /// <param name="key">The requested supplier identifier.</param>
    /// <returns>The requested supplier.</returns>
    /// <response code="200">The supplier was successfully retrieved.</response>
    /// <response code="404">The supplier does not exist.</response>
    [EnableQuery]
    [Produces( "application/json" )]
    [ProducesResponseType( typeof( Supplier ), Status200OK )]
    [ProducesResponseType( Status404NotFound )]
    public SingleResult<Supplier> Get( int key ) =>
        SingleResult.Create( suppliers.Where( p => p.Id == key ) );

    /// <summary>
    /// Creates a new supplier.
    /// </summary>
    /// <param name="supplier">The supplier to create.</param>
    /// <returns>The created supplier.</returns>
    /// <response code="201">The supplier was successfully created.</response>
    /// <response code="204">The product was successfully created and no content was preferred.</response>
    /// <response code="400">The supplier is invalid.</response>
    [Produces( "application/json" )]
    [ProducesResponseType( typeof( Supplier ), Status201Created )]
    [ProducesResponseType( Status204NoContent )]
    [ProducesResponseType( Status400BadRequest )]
    public IActionResult Post( [FromBody] Supplier supplier )
    {
        if ( !ModelState.IsValid )
        {
            return BadRequest( ModelState );
        }

        supplier.Id = 42;

        return Created( supplier );
    }

    /// <summary>
    /// Updates an existing supplier.
    /// </summary>
    /// <param name="key">The requested supplier identifier.</param>
    /// <param name="delta">The partial supplier to update.</param>
    /// <returns>The updated supplier.</returns>
    /// <response code="200">The supplier was successfully updated.</response>
    /// <response code="204">The supplier was successfully updated and no content was preferred.</response>
    /// <response code="400">The supplier is invalid.</response>
    /// <response code="404">The supplier does not exist.</response>
    [Produces( "application/json" )]
    [ProducesResponseType( typeof( Supplier ), Status200OK )]
    [ProducesResponseType( Status204NoContent )]
    [ProducesResponseType( Status400BadRequest )]
    [ProducesResponseType( Status404NotFound )]
    public IActionResult Patch( int key, [FromBody] Delta<Supplier> delta )
    {
        if ( !ModelState.IsValid )
        {
            return BadRequest( ModelState );
        }

        var supplier = new Supplier()
        {
            Id = key,
            Name = "Updated Supplier " + key.ToString(),
        };

        delta.Patch( supplier );

        return Updated( delta );
    }

    /// <summary>
    /// Updates an existing supplier.
    /// </summary>
    /// <param name="key">The requested supplier identifier.</param>
    /// <param name="update">The supplier to update.</param>
    /// <returns>The updated supplier.</returns>
    /// <response code="200">The supplier was successfully updated.</response>
    /// <response code="201">The supplier was successfully created.</response>
    /// <response code="400">The supplier is invalid.</response>
    /// <response code="404">The supplier does not exist.</response>
    [Produces( "application/json" )]
    [ProducesResponseType( typeof( Supplier ), Status200OK )]
    [ProducesResponseType( Status204NoContent )]
    [ProducesResponseType( Status400BadRequest )]
    [ProducesResponseType( Status404NotFound )]
    public IActionResult Put( int key, [FromBody] Supplier update )
    {
        if ( !ModelState.IsValid )
        {
            return BadRequest( ModelState );
        }

        update.Id = key;
        return Updated( update );
    }

    /// <summary>
    /// Deletes a supplier.
    /// </summary>
    /// <param name="key">The supplier to delete.</param>
    /// <returns>None.</returns>
    /// <response code="204">The supplier was successfully deleted.</response>
    [ProducesResponseType( Status204NoContent )]
    [ProducesResponseType( Status404NotFound )]
    public IActionResult Delete( int key ) => NoContent();

    /// <summary>
    /// Gets the products associated with the supplier.
    /// </summary>
    /// <param name="key">The supplier identifier.</param>
    /// <returns>The associated supplier products.</returns>
    [EnableQuery]
    [HttpGet( "api/Suppliers/{key}/Products" )]
    public IQueryable<Product> GetProducts( int key ) =>
        suppliers.Where( s => s.Id == key ).SelectMany( s => s.Products );

    /// <summary>
    /// Links a product to a supplier.
    /// </summary>
    /// <param name="key">The supplier identifier.</param>
    /// <param name="navigationProperty">The product to link.</param>
    /// <param name="link">The product identifier.</param>
    /// <returns>None.</returns>
    [ProducesResponseType( Status204NoContent )]
    [ProducesResponseType( Status404NotFound )]
    public IActionResult CreateRef(
        int key,
        string navigationProperty,
        [FromBody] Uri link ) => NoContent();

    /// <summary>
    /// Unlinks a product from a supplier.
    /// </summary>
    /// <param name="key">The supplier identifier.</param>
    /// <param name="relatedKey">The related product identifier.</param>
    /// <param name="navigationProperty">The product to unlink.</param>
    /// <returns>None.</returns>
    [ProducesResponseType( Status204NoContent )]
    [ProducesResponseType( Status404NotFound )]
    public IActionResult DeleteRef(
        int key,
        int relatedKey,
        string navigationProperty ) => NoContent();

    private static Supplier NewSupplier( int id ) =>
        new()
        {
            Id = id,
            Name = "Supplier " + id.ToString(),
            Products =
            {
                new()
                {
                    Id = id,
                    Name = "Product " + id.ToString(),
                    Category = "Test",
                    Price = id,
                    SupplierId = id,
                },
            },
        };
}