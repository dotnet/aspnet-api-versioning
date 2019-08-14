namespace Microsoft.AspNet.OData.Simulators.Models
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Product
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public decimal Price { get; set; }

        public string Category { get; set; }

        [ForeignKey( nameof( Supplier ) )]
        public int? SupplierId { get; set; }

        public virtual Supplier Supplier { get; set; }
    }
}