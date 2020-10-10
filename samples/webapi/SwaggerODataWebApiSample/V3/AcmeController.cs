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
    /// Represents a RESTful service for the ACME supplier.
    /// </summary>
    [ApiVersion( "3.0" )]
    public class AcmeController : ODataController
    {
        /// <summary>
        /// Retrieves the ACME supplier.
        /// </summary>
        /// <returns>The ACME supplier.</returns>
        /// <response code="200">The supplier was successfully retrieved.</response>
        [EnableQuery]
        [ResponseType( typeof( ODataValue<Supplier> ) )]
        public IHttpActionResult Get() => Ok( NewSupplier() );

        /// <summary>
        /// Gets the products associated with the supplier.
        /// </summary>
        /// <returns>The associated supplier products.</returns>
        [EnableQuery]
        public IQueryable<Product> GetProducts() => NewSupplier().Products.AsQueryable();

        /// <summary>
        /// Links a product to a supplier.
        /// </summary>
        /// <param name="navigationProperty">The product to link.</param>
        /// <param name="link">The product identifier.</param>
        /// <returns>None</returns>
        [HttpPost]
        public IHttpActionResult CreateRef( string navigationProperty, [FromBody] Uri link ) => StatusCode( NoContent );

        /// <summary>
        /// Unlinks a product from a supplier.
        /// </summary>
        /// <param name="relatedKey">The related product identifier.</param>
        /// <param name="navigationProperty">The product to unlink.</param>
        /// <returns>None</returns>
        public IHttpActionResult DeleteRef( [FromODataUri] string relatedKey, string navigationProperty ) => StatusCode( NoContent );

        private static Supplier NewSupplier() =>
            new Supplier()
            {
                Id = 42,
                Name = "Acme",
                Products = new List<Product>()
                {
                    new Product()
                    {
                        Id = 42,
                        Name = "Product 42",
                        Category = "Test",
                        Price = 42,
                        SupplierId = 42,
                    }
                },
            };
    }
}