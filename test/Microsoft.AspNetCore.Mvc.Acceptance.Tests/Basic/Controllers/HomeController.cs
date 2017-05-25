namespace Microsoft.AspNetCore.Mvc.Basic.Controllers
{
    using System;

    [ApiVersionNeutral]
    public class HomeController : Controller
    {
        public IActionResult Index() => View();
    }
}