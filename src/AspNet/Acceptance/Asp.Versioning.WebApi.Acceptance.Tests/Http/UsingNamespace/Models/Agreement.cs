// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Http.UsingNamespace.Models;

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