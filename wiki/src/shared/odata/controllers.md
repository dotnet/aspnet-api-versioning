# Versioned Controllers

Creating an OData controller that supports API versioning isn't much different from creating a regular OData controller.
The following controller depicts a service that support API version `1.0` and `2.0`.

```c#
[ApiVersion( 1.0 )]
[ApiVersion( 2.0 )]
public class PeopleController : ODataController
{
  // GET ~/people?api-version=[1.0|2.0]
  public IQueryable<Person> Get() => new[] { new Person() }.AsQueryable();

  // GET ~/people/1?api-version=[1.0|2.0]
  public SingleResult<Person> Get( int key ) => SingleResult.Create( new Person() );

  // PATCH ~/people/1?api-version=2.0
  [MapToApiVersion( 2.0 )]
  public UpdatedODataResult<Person> Patch( int key, Delta<Person> delta )
  {
      if ( !ModelState.IsValid )
      {
          return BadRequest( ModelState );
      }

      var person = new Person();
      delta.Patch( person );
      return Updated( person );
  }
}
```

The `PATCH` method is only supported in API version `2.0` of the service. To be truly OData compliant, this service
should define an action mapped to API version `1.0` that always returns HTTP status code `501` (Not Implemented) instead
of falling back to HTTP status code `400` (Bad Request) or `404` (Not Found).

If you reviewed the `Person` model and configuration example for the [IModelConfiguration], you'll know what we
configured a single `Person` model with different properties available in different API versions. The default OData
model validation does some automatic heavy lifting for us using the defined EDM model. In addition to the other normal
validation you might have from **Data Annotations**, the current EDM model will provide further validation. For example,
even though the `Person` class has a `Phone` property, it was not defined until API version `3.0`.  If you try to send
a `PATCH` request like this:

```http
PATCH /people/1?api-version=2.0 HTTP/2
content-type: application/json
content-length: 27

{ "phone": "555-555-5555" }
```

the built-in OData model validation will fail. The response will end up being HTTP status code `400` (Bad Request) with
an error message that indicates the `phone` property does not exist. In version `2.0` of the service, that is true and
the correct behavior.

## Split Implementation

Service authors can choose to split service API versions across multiple controller types. In fact, for all but the
simplest of version variations, this is the recommended approach. You may, however, notice something extra and a little
unusual about the attribution for this controller.

Under the hood, the OData implementation still uses convention-based routing. When we split services across multiple
controller types, the new service implementation cannot have the same name. The only exception to this rule is if you
create version-specific namespaces for each version of the service. If the name of the controller cannot be the same as
the original controller type and we're stuck with convention-based routing, how to do indicate what the name of the
controller should be? Enter the `ControllerNameAttribute`.

The `ControllerNameAttribute` allows you to specify an arbitrary name for a controller. In the strictest sense, this is
not convention-based; however, short of using different namespaces, there isn't a way to define the correct name of the
controller. Without the `ControllerNameAttribute`, this controller would be named **People2**, which won't match any
routes or, more specifically, any defined entity set. In OData, the controller route is paired with the corresponding
entity set name. The API version services honor this attribute and will use the controller name defined by the attribute
over the default convention name when present.

```c#
[ApiVersion( 3.0 )]
[ControllerName( "People" )]
public class People2Controller : ODataController
{
    // GET ~/people?api-version=3.0
    public IQueryable<Person> Get() => new[] { new Person() }.AsQueryable();

    // GET ~/people/1?api-version=3.0
    public SingleResult<Person> Get( int key ) => SingleResult.Create( new Person() );

    // PATCH ~/people/1?api-version=3.0
    public UpdatedODataResult<Person> Patch( int key, Delta<Person> delta )
    {
        if ( !ModelState.IsValid )
        {
            return BadRequest( ModelState );
        }

        var person = new Person();
        delta.Patch( person );
        return Updated( person );
    }
}
```

[IModelConfiguration]: model-config.md