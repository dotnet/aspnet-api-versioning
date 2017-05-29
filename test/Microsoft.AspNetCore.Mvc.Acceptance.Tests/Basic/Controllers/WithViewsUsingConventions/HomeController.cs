﻿namespace Microsoft.AspNetCore.Mvc.Basic.Controllers.WithViewsUsingConventions
{
    using System;

    [ApiVersionNeutral]
    public class HomeController : Controller
    {
        public IActionResult Index() => View();
    }
}