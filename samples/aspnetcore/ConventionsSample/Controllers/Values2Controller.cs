namespace Microsoft.Examples.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route( "api/values" )]
    public class Values2Controller : ControllerBase
    {
        // GET api/values?api-version=2.0
        [HttpGet]
        public string Get( ApiVersion apiVersion ) => $"Controller = {GetType().Name}\nVersion = {apiVersion}";

        // GET api/values/{id}?api-version=2.0
        [HttpGet( "{id:int}" )]
        public string Get( int id, ApiVersion apiVersion ) => $"Controller = {GetType().Name}\nId = {id}\nVersion = {apiVersion}";

        // GET api/values?api-version=3.0
        [HttpGet]
        public string GetV3( ApiVersion apiVersion ) => $"Controller = {GetType().Name}\nVersion = {apiVersion}";

        // GET api/values/{id}?api-version=3.0
        [HttpGet( "{id:int}" )]
        public string GetV3( int id, ApiVersion apiVersion ) => $"Controller = {GetType().Name}\nId = {id}\nVersion = {apiVersion}";
    }
}