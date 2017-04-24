using Microsoft.AspNetCore.Mvc;
using SwaggerSampleHeader.V2.Models;

namespace SwaggerSampleHeader.V2.Controllers
{
    [ApiVersion("2.0")]
    [Route("api/[controller]")]
    public class CustomersController : Controller
    {
        [HttpGet("{id:int}")]
        [MapToApiVersion("2.0")]
        [ProducesResponseType(typeof(Customer), 200)]
        [ProducesResponseType(404)]
        public IActionResult Get(int id)
        {
            return Ok(new Customer() { Id = 1, FirstName = "Luis", LastName= "Ruiz" });
        }
    }
}
