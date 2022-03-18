// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Mvc.UsingAttributes.Controllers.WithViewsUsingAttributes;

using Microsoft.AspNetCore.Mvc;

[Route( "" )]
[Route( "[controller]" )]
public class HomeController : Controller
{
    [HttpGet( "" )]
    [HttpGet( nameof( Index ) )]
    public IActionResult Index() => View();
}