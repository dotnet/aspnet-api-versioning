<!-- description: The ODataApiExplorerOptions settings for ASP.NET Core, including qualified names, query options, and metadata. -->

{{#include ../../shared/docs/odata-options-pre.md}}

{{#include ../../shared/docs/odata-options-post.md}}

{{#include ../../shared/docs/odata-options-query.md}}

{{#include ../../shared/docs/odata-options-attributes.md}}

```c#
using Asp.Versioning;
using Asp.Versioning.OData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using static Microsoft.AspNetCore.Http.StatusCodes;
using static Microsoft.AspNetCore.OData.Query.AllowedQueryOptions;
using static System.DateTime;

[ApiVersion( 1.0 )]
public class OrdersController : ODataController
{
    [Produces( "application/json" )]
    [ProducesResponseType( typeof( ODataValue<IEnumerable<Order>> ), Status200OK )]
    [EnableQuery( MaxTop = 100, AllowedQueryOptions = Select | Top | Skip | Count )]
    public IQueryable<Order> Get()
    {
      var orders = new[]
      {
        new Order(){ Id = 1, Customer = "John Doe" },
        new Order(){ Id = 2, Customer = "John Doe" },
        new Order(){ Id = 3, Customer = "Jane Doe", EffectiveDate = UtcNow.AddDays(7d) }
      };

      return orders.AsQueryable();
    }

    [Produces( "application/json" )]
    [ProducesResponseType( typeof( Order ), Status200OK )]
    [ProducesResponseType( Status404NotFound )]
    [EnableQuery( AllowedQueryOptions = Select )]
    public SingleResult<Order> Get( int key )
    {
      var orders = new[] { new Order(){ Id = key, Customer = "John Doe" } };
      return SingleResult.Create( orders.AsQueryable() );
    }
}
```

{{#include ../../shared/docs/odata-options-model-bound.md}}

```c#
using Asp.Versioning;
using Asp.Versioning.OData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using static Microsoft.AspNetCore.Http.StatusCodes;
using static Microsoft.AspNetCore.OData.Query.AllowedQueryOptions;
using static System.DateTime;

public class PeopleController : ODataController
{
    [Produces( "application/json" )]
    [ProducesResponseType( typeof( ODataValue<IEnumerable<Person>> ), Status200OK )]
    public IActionResult Get( ODataQueryOptions<Person> options )
    {
        var validationSettings = new ODataValidationSettings()
        {
            AllowedQueryOptions = Select | OrderBy | Top | Skip | Count,
            AllowedOrderByProperties = { "firstName", "lastName" },
            AllowedArithmeticOperators = AllowedArithmeticOperators.None,
            AllowedFunctions = AllowedFunctions.None,
            AllowedLogicalOperators = AllowedLogicalOperators.None,
            MaxOrderByNodeCount = 2,
            MaxTop = 100,
        };

        try
        {
            options.Validate( validationSettings );
        }
        catch ( ODataException )
        {
            return BadRequest();
        }

        var people = new[]
        {
            new Person()
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@somewhere.com",
                Phone = "555-987-1234",
            },
            new Person()
            {
                Id = 2,
                FirstName = "Bob",
                LastName = "Smith",
                Email = "bob.smith@somewhere.com",
                Phone = "555-654-4321",
            },
            new Person()
            {
                Id = 3,
                FirstName = "Jane",
                LastName = "Doe",
                Email = "jane.doe@somewhere.com",
                Phone = "555-789-3456",
            }
        };

        return Ok( options.ApplyTo( people.AsQueryable() ) );
    }

    [Produces( "application/json" )]
    [ProducesResponseType( typeof( Person ), Status200OK )]
    [ProducesResponseType( Status404NotFound )]
    public IActionResult Get( int key, ODataQueryOptions<Person> options )
    {
        var people = new[]
        {
            new Person()
            {
                Id = key,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@somewhere.com",
                Phone = "555-987-1234",
            }
        };

        var person = options.ApplyTo( people.AsQueryable() ).SingleOrDefault();

        if ( person == null )
        {
            return NotFound();
        }

        return Ok( person );
    }
}
```

{{#include ../../shared/docs/odata-options-mid.md}}

{{#include ../../shared/docs/odata-options-partial-pre.md}}

```c#
[ApiVersion( 1.0 )]
[ApiController]
[Route( "[controller]" )]
public class BooksController : ControllerBase
{
    [HttpGet]
    [Produces( "application/json" )]
    [ProducesResponseType( typeof( IEnumerable<Book> ), 200 )]
    public IActionResult Get( ODataQueryOptions<Book> options ) =>
        Ok( options.ApplyTo( books.AsQueryable() ) );
}
```

{{#include ../../shared/docs/odata-options-partial-post.md}}