namespace Microsoft.Examples.V1.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Represents a person.
    /// </summary>
    public class Person
    {
        /// <summary>
        /// Gets or sets the unique identifier for a person.
        /// </summary>
        /// <value>The person's unique identifier.</value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the first name of a person.
        /// </summary>
        /// <value>The person's first name.</value>
        [Required]
        [StringLength( 25 )]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name of a person.
        /// </summary>
        /// <value>The person's last name.</value>
        [Required]
        [StringLength( 25 )]
        public string LastName { get; set; }
    }
}