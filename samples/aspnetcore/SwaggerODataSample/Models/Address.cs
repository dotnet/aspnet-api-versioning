namespace Microsoft.Examples.Models
{
    using Microsoft.AspNet.OData.Query;
    using System;

    /// <summary>
    /// Represents an address.
    /// </summary>
    public class Address
    {
        /// <summary>
        /// Gets or sets the address identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the street address.
        /// </summary>
        /// <value>The street address.</value>
        public string Street { get; set; }

        /// <summary>
        /// Gets or sets the address city.
        /// </summary>
        /// <value>The address city.</value>
        public string City { get; set; }

        /// <summary>
        /// Gets or sets the address state.
        /// </summary>
        /// <value>The address state.</value>
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the address zip code.
        /// </summary>
        /// <value>The address zip code.</value>
        public string ZipCode { get; set; }
    }
}