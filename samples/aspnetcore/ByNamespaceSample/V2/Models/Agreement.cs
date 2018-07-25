﻿namespace Microsoft.Examples.V2.Models
{
    using System;

    public class Agreement
    {
        public Agreement( string controller, string accountId, string apiVersion )
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