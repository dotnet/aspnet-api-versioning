namespace Microsoft.Web.Http.Simulators.V3
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Extensions;
    using Microsoft.Web.Http.Description;
    using Microsoft.Web.Http.Simulators.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Web.Http;
    using System.Web.Http.Description;
    using static System.Linq.Enumerable;
    using static System.Net.HttpStatusCode;

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
        [ResponseType( typeof( ODataValue<IEnumerable<Product>> ) )]
        public IQueryable<Product> Get() => products;

        /// <summary>
        /// Gets a single product.
        /// </summary>
        /// <param name="key">The requested product identifier.</param>
        /// <returns>The requested product.</returns>
        /// <response code="200">The product was successfully retrieved.</response>
        /// <response code="404">The product does not exist.</response>
        [EnableQuery]
        [ResponseType( typeof( Product ) )]
        public SingleResult<Product> Get( [FromODataUri] int key ) => SingleResult.Create( products.Where( p => p.Id == key ) );

        /// <summary>
        /// Creates a new product.
        /// </summary>
        /// <param name="product">The product to create.</param>
        /// <returns>The created product.</returns>
        /// <response code="201">The product was successfully created.</response>
        /// <response code="204">The product was successfully created.</response>
        /// <response code="400">The product is invalid.</response>
        [ResponseType( typeof( Product ) )]
        public IHttpActionResult Post( [FromBody] Product product )
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
        [ResponseType( typeof( Product ) )]
        public IHttpActionResult Patch( [FromODataUri] int key, Delta<Product> delta )
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
        [ResponseType( typeof( Product ) )]
        public IHttpActionResult Put( [FromODataUri] int key, [FromBody] Product update )
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
        public IHttpActionResult Delete( [FromODataUri] int key ) => StatusCode( NoContent );

        /// <summary>
        /// Gets the supplier associated with the product.
        /// </summary>
        /// <param name="key">The product identifier.</param>
        /// <returns>The supplier</returns>
        /// <returns>The requested supplier.</returns>
        [EnableQuery]
        [ResponseType( typeof( Supplier ) )]
        public SingleResult<Supplier> GetSupplier( [FromODataUri] int key ) => SingleResult.Create( products.Where( p => p.Id == key ).Select( p => p.Supplier ) );

        /// <summary>
        /// Gets the link to the associated supplier, if any.
        /// </summary>
        /// <param name="key">The product identifier.</param>
        /// <param name="navigationProperty">The supplier to link.</param>
        /// <returns>The supplier link.</returns>
        [ResponseType( typeof( ODataId ) )]
        public IHttpActionResult GetRefToSupplier( [FromODataUri] int key, string navigationProperty )
        {
            var routeName = Request.ODataProperties().RouteName;
            var url = Request.RequestUri;
            var builder = new StringBuilder( url.Scheme );

            builder.Append( Uri.SchemeDelimiter );
            builder.Append( url.Host );

            if ( !string.IsNullOrEmpty( routeName ) )
            {
                builder.Append( '/' );
                builder.Append( routeName );
            }

            builder.Append( "/Products/" );
            builder.Append( key );
            builder.Append( '/' );
            builder.Append( navigationProperty );

            return Ok( new Uri( builder.ToString() ) );
        }

        /// <summary>
        /// Links a supplier to a product.
        /// </summary>
        /// <param name="key">The product identifier.</param>
        /// <param name="navigationProperty">The supplier to link.</param>
        /// <param name="link">The supplier identifier.</param>
        /// <returns>None</returns>
        [HttpPut]
        public IHttpActionResult CreateRefToSupplier( [FromODataUri] int key, string navigationProperty, [FromBody] Uri link ) => StatusCode( NoContent );

        /// <summary>
        /// Unlinks a supplier from a product.
        /// </summary>
        /// <param name="key">The product identifier.</param>
        /// <param name="navigationProperty">The supplier to unlink.</param>
        /// <returns>None</returns>
        public IHttpActionResult DeleteRefToSupplier( [FromODataUri] int key, string navigationProperty ) => StatusCode( NoContent );

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