namespace Microsoft.Examples.V3
{
    using Microsoft.AspNet.OData;
    using Microsoft.Examples.Models;
    using Microsoft.Web.Http;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Http;
    using System.Web.Http.Description;
    using static System.Net.HttpStatusCode;

    /// <summary>
    /// Represents a RESTful service of suppliers.
    /// </summary>
    [ApiVersion( "3.0" )]
    public class SuppliersController : ODataController
    {
        private readonly IQueryable<Supplier> suppliers = new[] { NewSupplier( 1 ), NewSupplier( 2 ), NewSupplier( 3 ), }.AsQueryable();

        /// <summary>
        /// Retrieves all suppliers.
        /// </summary>
        /// <returns>All available suppliers.</returns>
        /// <response code="200">Products successfully retrieved.</response>
        [EnableQuery]
        [ResponseType( typeof( ODataValue<IEnumerable<Supplier>> ) )]
        public IQueryable<Supplier> Get() => suppliers;

        /// <summary>
        /// Gets a single supplier.
        /// </summary>
        /// <param name="key">The requested supplier identifier.</param>
        /// <returns>The requested supplier.</returns>
        /// <response code="200">The supplier was successfully retrieved.</response>
        /// <response code="404">The supplier does not exist.</response>
        [EnableQuery]
        [ResponseType( typeof( Supplier ) )]
        public SingleResult<Supplier> Get( [FromODataUri] int key ) => SingleResult.Create( suppliers.Where( p => p.Id == key ) );

        /// <summary>
        /// Creates a new supplier.
        /// </summary>
        /// <param name="supplier">The supplier to create.</param>
        /// <returns>The created supplier.</returns>
        /// <response code="201">The supplier was successfully created.</response>
        /// <response code="204">The supplier was successfully created.</response>
        /// <response code="400">The supplier is invalid.</response>
        [ResponseType( typeof( Supplier ) )]
        public IHttpActionResult Post( [FromBody] Supplier supplier )
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
        /// <response code="204">The supplier was successfully updated.</response>
        /// <response code="400">The supplier is invalid.</response>
        /// <response code="404">The supplier does not exist.</response>
        [ResponseType( typeof( Supplier ) )]
        public IHttpActionResult Patch( [FromODataUri] int key, Delta<Supplier> delta )
        {
            if ( !ModelState.IsValid )
            {
                return BadRequest( ModelState );
            }

            var supplier = new Supplier() { Id = key, Name = "Updated Supplier " + key.ToString() };

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
        /// <response code="204">The supplier was successfully updated.</response>
        /// <response code="400">The supplier is invalid.</response>
        /// <response code="404">The supplier does not exist.</response>
        [ResponseType( typeof( Supplier ) )]
        public IHttpActionResult Put( [FromODataUri] int key, [FromBody] Supplier update )
        {
            if ( !ModelState.IsValid )
            {
                return BadRequest( ModelState );
            }

            return Updated( update );
        }

        /// <summary>
        /// Deletes a supplier.
        /// </summary>
        /// <param name="key">The supplier to delete.</param>
        /// <returns>None</returns>
        /// <response code="204">The supplier was successfully deleted.</response>
        public IHttpActionResult Delete( [FromODataUri] int key ) => StatusCode( NoContent );

        /// <summary>
        /// Gets the products associated with the supplier.
        /// </summary>
        /// <param name="key">The supplier identifier.</param>
        /// <returns>The associated supplier products.</returns>
        [EnableQuery]
        public IQueryable<Product> GetProducts( [FromODataUri] int key ) => suppliers.Where( s => s.Id == key ).SelectMany( s => s.Products );

        /// <summary>
        /// Links a product to a supplier.
        /// </summary>
        /// <param name="key">The supplier identifier.</param>
        /// <param name="navigationProperty">The product to link.</param>
        /// <param name="link">The product identifier.</param>
        /// <returns>None</returns>
        [HttpPost]
        public IHttpActionResult CreateRefToProducts( [FromODataUri] int key, string navigationProperty, [FromBody] Uri link ) => StatusCode( NoContent );

        /// <summary>
        /// Unlinks a product from a supplier.
        /// </summary>
        /// <param name="key">The supplier identifier.</param>
        /// <param name="relatedKey">The related product identifier.</param>
        /// <param name="navigationProperty">The product to unlink.</param>
        /// <returns>None</returns>
        public IHttpActionResult DeleteRefToProducts( [FromODataUri] int key, [FromODataUri] string relatedKey, string navigationProperty ) => StatusCode( NoContent );

        private static Supplier NewSupplier( int id ) =>
            new Supplier()
            {
                Id = id,
                Name = "Supplier " + id.ToString(),
                Products = new List<Product>()
                {
                    new Product()
                    {
                        Id = id,
                        Name = "Product "  + id.ToString(),
                        Category = "Test",
                        Price = id,
                        SupplierId = id,
                    }
                },
            };
    }
}