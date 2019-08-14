namespace Microsoft.AspNet.OData.Simulators.V3
{
    using Microsoft.AspNet.OData.Extensions;
    using Microsoft.AspNet.OData.Simulators.Models;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.OData.UriParser;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using static Microsoft.AspNetCore.Http.StatusCodes;

    /// <summary>
    /// Represents a RESTful service of products.
    /// </summary>
    [ApiVersion( "3.0" )]
    public class ProductsController : ODataController
    {
        private readonly IQueryable<Product> products = new[] { NewProduct( 1 ), NewProduct( 2 ), NewProduct( 3 ), }.AsQueryable();

        /// <summary>
        /// Retrieves all products.
        /// </summary>
        /// <returns>All available products.</returns>
        /// <response code="200">Products successfully retrieved.</response>
        [EnableQuery]
        [Produces( "application/json" )]
        [ProducesResponseType( typeof( ODataValue<IEnumerable<Product>> ), Status200OK )]
        public IQueryable<Product> Get() => products;

        /// <summary>
        /// Gets a single product.
        /// </summary>
        /// <param name="key">The requested product identifier.</param>
        /// <returns>The requested product.</returns>
        /// <response code="200">The product was successfully retrieved.</response>
        /// <response code="404">The product does not exist.</response>
        [EnableQuery]
        [Produces( "application/json" )]
        [ProducesResponseType( typeof( Product ), Status200OK )]
        [ProducesResponseType( Status404NotFound )]
        public SingleResult<Product> Get( [FromODataUri] int key ) => SingleResult.Create( products.Where( p => p.Id == key ) );

        /// <summary>
        /// Creates a new product.
        /// </summary>
        /// <param name="product">The product to create.</param>
        /// <returns>The created product.</returns>
        /// <response code="201">The product was successfully created.</response>
        /// <response code="204">The product was successfully created.</response>
        /// <response code="400">The product is invalid.</response>
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
        /// Updates an existing product.
        /// </summary>
        /// <param name="key">The requested product identifier.</param>
        /// <param name="delta">The partial product to update.</param>
        /// <returns>The updated product.</returns>
        /// <response code="200">The product was successfully updated.</response>
        /// <response code="204">The product was successfully updated.</response>
        /// <response code="400">The product is invalid.</response>
        /// <response code="404">The product does not exist.</response>
        [Produces( "application/json" )]
        [ProducesResponseType( typeof( Product ), Status200OK )]
        [ProducesResponseType( Status204NoContent )]
        [ProducesResponseType( Status400BadRequest )]
        [ProducesResponseType( Status404NotFound )]
        public IActionResult Patch( [FromODataUri] int key, Delta<Product> delta )
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
        /// Updates an existing product.
        /// </summary>
        /// <param name="key">The requested product identifier.</param>
        /// <param name="update">The product to update.</param>
        /// <returns>The updated product.</returns>
        /// <response code="200">The product was successfully updated.</response>
        /// <response code="204">The product was successfully updated.</response>
        /// <response code="400">The product is invalid.</response>
        /// <response code="404">The product does not exist.</response>
        [Produces( "application/json" )]
        [ProducesResponseType( typeof( Product ), Status200OK )]
        [ProducesResponseType( Status204NoContent )]
        [ProducesResponseType( Status400BadRequest )]
        [ProducesResponseType( Status404NotFound )]
        public IActionResult Put( [FromODataUri] int key, [FromBody] Product update )
        {
            if ( !ModelState.IsValid )
            {
                return BadRequest( ModelState );
            }

            return Updated( update );
        }

        /// <summary>
        /// Deletes a product.
        /// </summary>
        /// <param name="key">The product to delete.</param>
        /// <returns>None</returns>
        /// <response code="204">The product was successfully deleted.</response>
        [ProducesResponseType( Status204NoContent )]
        [ProducesResponseType( Status404NotFound )]
        public IActionResult Delete( [FromODataUri] int key ) => NoContent();

        /// <summary>
        /// Gets the supplier associated with the product.
        /// </summary>
        /// <param name="key">The product identifier.</param>
        /// <returns>The supplier</returns>
        /// <returns>The requested supplier.</returns>
        [EnableQuery]
        [Produces( "application/json" )]
        [ProducesResponseType( typeof( Supplier ), Status200OK )]
        [ProducesResponseType( Status404NotFound )]
        public SingleResult<Supplier> GetSupplier( [FromODataUri] int key ) => SingleResult.Create( products.Where( p => p.Id == key ).Select( p => p.Supplier ) );

        /// <summary>
        /// Gets the link to the associated supplier, if any.
        /// </summary>
        /// <param name="key">The product identifier.</param>
        /// <param name="navigationProperty">The supplier to link.</param>
        /// <returns>The supplier link.</returns>
        [Produces( "application/json" )]
        [ProducesResponseType( typeof( ODataId ), Status200OK )]
        [ProducesResponseType( Status404NotFound )]
        public IActionResult GetRefToSupplier( [FromODataUri] int key, [FromODataUri] string navigationProperty )
        {
            var segments = Request.ODataFeature().Path.Segments.ToArray();
            var entitySet = ( (EntitySetSegment) segments[0] ).EntitySet;
            var property = entitySet.NavigationPropertyBindings.Single( p => p.Path.Path == navigationProperty ).NavigationProperty;

            segments[segments.Length - 1] = new NavigationPropertySegment( property, entitySet );

            var relatedKey = new Uri( Url.CreateODataLink( segments ) );

            return Ok( relatedKey );
        }

        /// <summary>
        /// Links a supplier to a product.
        /// </summary>
        /// <param name="key">The product identifier.</param>
        /// <param name="navigationProperty">The supplier to link.</param>
        /// <param name="link">The supplier identifier.</param>
        /// <returns>None</returns>
        [HttpPut]
        [ProducesResponseType( Status204NoContent )]
        [ProducesResponseType( Status404NotFound )]
        public IActionResult CreateRefToSupplier( [FromODataUri] int key, [FromODataUri] string navigationProperty, [FromBody] Uri link ) => NoContent();

        /// <summary>
        /// Unlinks a supplier from a product.
        /// </summary>
        /// <param name="key">The product identifier.</param>
        /// <param name="navigationProperty">The supplier to unlink.</param>
        /// <returns>None</returns>
        [ProducesResponseType( Status204NoContent )]
        [ProducesResponseType( Status404NotFound )]
        public IActionResult DeleteRefToSupplier( [FromODataUri] int key, [FromODataUri] string navigationProperty ) => NoContent();

        static Product NewProduct( int id ) =>
            new Product()
            {
                Id = id,
                Category = "Test",
                Name = "Product " + id.ToString(),
                Price = id,
                Supplier = new Supplier() { Id = id, Name = "Supplier " + id.ToString() },
                SupplierId = id,
            };
    }
}