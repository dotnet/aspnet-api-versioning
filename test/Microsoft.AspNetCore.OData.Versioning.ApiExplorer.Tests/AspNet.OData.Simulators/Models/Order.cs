namespace Microsoft.AspNet.OData.Simulators.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Represents an order.
    /// </summary>
    public class Order
    {
        /// <summary>
        /// Gets or sets the unique identifier for the order.
        /// </summary>
        /// <value>The order's unique identifier.</value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the order was created.
        /// </summary>
        /// <value>The order's creation date.</value>
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Gets or sets the date and time when the order becomes effective.
        /// </summary>
        /// <value>The order's effective date.</value>
        public DateTime EffectiveDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Gets or sets the name of the ordering customer.
        /// </summary>
        /// <value>The name of the customer that placed the order.</value>
        [Required]
        public string Customer { get; set; }

        /// <summary>
        /// Gets or sets a description for the order.
        /// </summary>
        /// <value>The description of the order.</value>
        public string Description { get; set; }
    }
}