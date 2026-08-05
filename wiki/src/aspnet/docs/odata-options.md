{{#include ../../shared/docs/odata-options-pre.md}}
- [UseApiExplorerSettings](#use-api-explorer-settings)<sup>1</sup>

### Use API Explorer Settings

OData controllers are not explored by default. The API explorer for OData services does not initially honor this
setting so that OData APIs will be discovered. You might decide, however, to use the API explorer settings to explicitly
define which OData services should be explored. You must set this property to a value of `true` in order for the API
explorer to respect API explorer settings.

{{#include ../../shared/docs/odata-options-post.md}}

{{#include ../../shared/docs/odata-options-query.md}}

{{#include ../../shared/docs/odata-options-attributes.md}}

```c#
using Asp.Versioning;
using Asp.Versioning.OData;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Microsoft.Web.Http;
using System.Web.Http;
using System.Web.Http.Description;
using static Microsoft.AspNet.OData.Query.AllowedQueryOptions;
using static System.Net.HttpStatusCode;
using static System.DateTime;

[ApiVersion( 1.0 )]
[ODataRoutePrefix( "Orders" )]
public class OrdersController : ODataController
{
    [ODataRoute]
    [Produces( "application/json" )]
    [ProducesResponseType( typeof( ODataValue<IEnumerable<Order>> ), Status200OK )]
    [EnableQuery( MaxTop = 100, AllowedQueryOptions = Select | Top | Skip | Count )]
    public IQueryable<Order> Get()
    {
      var orders = new[]
      {
        new Order(){ Id = 1, Customer = "John Doe" },
        new Order(){ Id = 2, Customer = "John Doe" },
        new Order(){ Id = 3, Customer = "Jane Doe", EffectiveDate = UtcNow.AddDays( 7d ) }
      };

      return orders.AsQueryable();
    }

    [ODataRoute( "{key}" )]
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
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Microsoft.Web.Http;
using System.Web.Http;
using System.Web.Http.Description;
using static Microsoft.AspNet.OData.Query.AllowedQueryOptions;
using static System.Net.HttpStatusCode;
using static System.DateTime;

public class PeopleController : ODataController
{
    [HttpGet]
    [ResponseType( typeof( ODataValue<IEnumerable<Person>> ) )]
    public IHttpActionResult Get( ODataQueryOptions<Person> options )
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

        return this.Success( options.ApplyTo( people.AsQueryable() ) );
    }

    [HttpGet]
    [ResponseType( typeof( Person ) )]
    public IHttpActionResult Get( int key, ODataQueryOptions<Person> options )
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

        var query = options.ApplyTo( people.AsQueryable();
        return this.SuccessOrNotFound( query ).SingleOrDefault() );
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