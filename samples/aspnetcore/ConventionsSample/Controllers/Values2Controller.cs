namespace Microsoft.Examples.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    [Route( "api/values" )]
    public class Values2Controller : Controller
    {
        // GET api/values?api-version=2.0
        [HttpGet]
        public string Get() => $"Controller = {GetType().Name}\nVersion = {HttpContext.GetRequestedApiVersion()}";

        // GET api/values/{id}?api-version=2.0
        [HttpGet( "{id:int}" )]
        public string Get( int id ) => $"Controller = {GetType().Name}\nId = {id}\nVersion = {HttpContext.GetRequestedApiVersion()}";

        // GET api/values?api-version=3.0
        [HttpGet]
        public string GetV3() => $"Controller = {GetType().Name}\nVersion = {HttpContext.GetRequestedApiVersion()}";

        // GET api/values/{id}?api-version=3.0
        [HttpGet( "{id:int}" )]
        public string GetV3( int id ) => $"Controller = {GetType().Name}\nId = {id}\nVersion = {HttpContext.GetRequestedApiVersion()}";
    }
}