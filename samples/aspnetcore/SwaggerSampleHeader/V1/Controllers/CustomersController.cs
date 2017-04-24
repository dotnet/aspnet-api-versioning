using Microsoft.AspNetCore.Mvc;
using SwaggerSampleHeader.V1.Models;

namespace SwaggerSampleHeader.V1.Controllers
{
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [Route("api/[controller]")]
    public class CustomersController : Controller
    {
        [HttpGet("{id:int}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(Customer), 200)]
        [ProducesResponseType(404)]
        public IActionResult Get(int id)
        {
            return Ok(new Customer() { Id = 1, FullName = "Luis Ruiz" });
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(Customer), 204)]
        [ProducesResponseType(404)]
        public IActionResult Delete(int id)
        {
            return NoContent();
        }
    }
}
