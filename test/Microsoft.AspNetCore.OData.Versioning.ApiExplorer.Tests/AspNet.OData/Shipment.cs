namespace Microsoft.AspNet.OData
{
    using System;

    public class Shipment
    {
        public int Id { get; set; }
        
        public DateTime ShippedOn { get; set; }
        
        public double Weight { get; set; }
        
        public Address Destination { get; set; }
    }
}