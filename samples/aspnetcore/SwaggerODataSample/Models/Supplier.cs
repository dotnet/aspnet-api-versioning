namespace Microsoft.Examples.Models
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a supplier.
    /// </summary>
    public class Supplier
    {
        /// <summary>
        /// Gets or sets the unique identifier for the supplier.
        /// </summary>
        /// <value>The supplier's unique identifier.</value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the supplier name.
        /// </summary>
        /// <value>The supplier's name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets products associated with the supplier.
        /// </summary>
        /// <value>The collection of associated products.</value>
        public ICollection<Product> Products { get; set; }
    }
}