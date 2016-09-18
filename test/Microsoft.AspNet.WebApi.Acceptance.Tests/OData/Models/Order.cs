namespace Microsoft.Web.OData.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Web;

    public class Order
    {
        public int Id { get; set;  }

        public DateTimeOffset CreatedDate { get; set; } = DateTimeOffset.Now;

        public DateTimeOffset EffectiveDate { get; set; } = DateTimeOffset.Now;

        [Required]
        public string Customer { get; set; }
    }
}