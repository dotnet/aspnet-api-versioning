namespace ApiVersioning.Examples.Controllers;

using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

[ApiVersion( 1.0 )]
[Route( "api/[controller]" )]
public class ValuesController : ControllerBase
{
    // GET api/values?api-version=1.0
    [HttpGet]
    public string Get( ApiVersion apiVersion ) => $"Controller = {GetType().Name}\nVersion = {apiVersion}";
}