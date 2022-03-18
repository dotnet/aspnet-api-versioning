// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Mvc.UsingAttributes.Controllers.WithViewsUsingConventions;

using Microsoft.AspNetCore.Mvc;

public class HomeController : Controller
{
    public IActionResult Index() => View();
}