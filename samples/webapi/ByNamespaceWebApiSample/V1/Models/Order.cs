namespace Microsoft.Examples.V1.Models
{
    using System;

    public class Order
    {
        public Order( string controller, string accountId, string apiVersion )
        {
            Controller = controller;
            AccountId = accountId;
            ApiVersion = apiVersion;
        }

        public string Controller { get; set; }

        public string AccountId { get; set; }

        public string ApiVersion { get; set; }
    }
}