using ApiVersioning.Examples.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace ApiVersioning.Examples.Controllers
{
    [Route("[controller]")]
    public class PeopleController : ODataController
    {
        [HttpGet]
        public IEnumerable<Person> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new Person
            {
                PersonId = index,
                FirstName = $"First{index}",
                LastName = $"Last{index}"
            })
            .ToArray();
        }
    }
}
