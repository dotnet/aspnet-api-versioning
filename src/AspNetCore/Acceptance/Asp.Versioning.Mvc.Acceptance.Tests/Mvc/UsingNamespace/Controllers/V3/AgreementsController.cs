// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Mvc.UsingNamespace.Controllers.V3;

using Asp.Versioning.Mvc.UsingNamespace.Models;
using Microsoft.AspNetCore.Mvc;

public class AgreementsController : ControllerBase
{
    [HttpGet]
    public IActionResult Get( string accountId, ApiVersion apiVersion ) =>
        Ok( new Agreement( GetType().FullName, accountId, apiVersion.ToString() ) );
}