namespace Microsoft.Examples.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    [ApiVersion( "1.0" )]
    [Route( "api/v{version:apiVersion}/[controller]" )]
    public class HelloWorldController : Controller
    {
        // GET api/v{version}/helloworld
        [HttpGet]
        public string Get() => $"Controller = {GetType().Name}\nVersion = {HttpContext.GetRequestedApiVersion()}";
    }
}
