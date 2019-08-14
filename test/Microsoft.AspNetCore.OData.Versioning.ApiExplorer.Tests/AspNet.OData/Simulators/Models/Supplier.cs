namespace Microsoft.AspNet.OData.Simulators.Models
{
    using System;
    using System.Collections.Generic;

    public class Supplier
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public ICollection<Product> Products { get; set; }
    }
}