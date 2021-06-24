namespace Microsoft.Examples.V3
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Examples.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using static Microsoft.AspNetCore.Http.StatusCodes;

    /// <summary>
    /// Represents a RESTful service for the ACME supplier.
    /// </summary>
    [ApiVersion( "3.0" )]
    public class AcmeController : ODataController
    {
        /// <summary>
        /// Retrieves the ACME supplier.
        /// </summary>
        /// <returns>All available suppliers.</returns>
        /// <response code="200">The supplier successfully retrieved.</response>
        [EnableQuery]
        [Produces( "application/json" )]
        [ProducesResponseType( typeof( ODataValue<Supplier> ), Status200OK )]
        public IActionResult Get() => Ok( NewSupplier() );

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
        [ProducesResponseType( Status204NoContent )]
        [ProducesResponseType( Status404NotFound )]
        public IActionResult CreateRef( [FromODataUri] string navigationProperty, [FromBody] Uri link ) => NoContent();

        // TODO: OData doesn't seem to currently support this action in ASP.NET Core, but it works in Web API

        /// <summary>
        /// Unlinks a product from a supplier.
        /// </summary>
        /// <param name="relatedKey">The related product identifier.</param>
        /// <param name="navigationProperty">The product to unlink.</param>
        /// <returns>None</returns>
        [ProducesResponseType( Status204NoContent )]
        [ProducesResponseType( Status404NotFound )]
        public IActionResult DeleteRef( [FromODataUri] string relatedKey, string navigationProperty ) => NoContent();

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
