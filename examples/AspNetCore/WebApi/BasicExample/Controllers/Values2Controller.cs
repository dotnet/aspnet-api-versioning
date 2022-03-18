namespace ApiVersioning.Examples.Controllers;

using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

[ApiVersion( 2.0 )]
[Route( "api/values" )]
public class Values2Controller : ControllerBase
{
    // GET api/values?api-version=2.0
    [HttpGet]
    public string Get( ApiVersion apiVersion ) => $"Controller = {GetType().Name}\nVersion = {apiVersion}";
}