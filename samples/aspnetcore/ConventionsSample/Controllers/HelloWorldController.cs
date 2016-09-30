namespace Microsoft.Examples.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    [Route( "api/v{version:apiVersion}/[controller]" )]
    public class HelloWorldController : Controller
    {
        // GET api/v{version}/helloworld
        [HttpGet]
        public string Get() => $"Controller = {GetType().Name}\nVersion = {HttpContext.GetRequestedApiVersion()}";

        // GET api/v{version}/helloworld/{id}
        [HttpGet( "{id:int}" )]
        public string Get( int id ) => $"Controller = {GetType().Name}\nId = {id}\nVersion = {HttpContext.GetRequestedApiVersion()}";
    }
}