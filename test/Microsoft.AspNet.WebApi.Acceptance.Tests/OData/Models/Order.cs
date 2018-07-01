namespace Microsoft.AspNet.OData.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class Order
    {
        public int Id { get; set;  }

        public DateTimeOffset CreatedDate { get; set; } = DateTimeOffset.Now;

        public DateTimeOffset EffectiveDate { get; set; } = DateTimeOffset.Now;

        [Required]
        public string Customer { get; set; }
    }
}