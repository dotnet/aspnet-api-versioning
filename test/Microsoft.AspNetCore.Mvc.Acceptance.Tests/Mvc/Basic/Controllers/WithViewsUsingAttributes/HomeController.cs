namespace Microsoft.AspNetCore.Mvc.Basic.Controllers.WithViewsUsingAttributes
{
    using System;

    [Route( "" )]
    [Route( "[controller]" )]
    public class HomeController : Controller
    {
        [HttpGet( "" )]
        [HttpGet( nameof( Index ) )]
        public IActionResult Index() => View();
    }
}