namespace Microsoft.AspNetCore.Mvc.Basic.Controllers.WithViewsUsingConventions
{
    using System;

    public class HomeController : Controller
    {
        public IActionResult Index() => View();
    }
}