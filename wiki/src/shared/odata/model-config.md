# Model Configurations

A model configuration enables OData service authors to apply model setups that are specific to a service API version.
The [VersionedODataModelBuilder] will call `Apply` for each discovered API version with the current `ODataModelBuilder`.

 ```c#
public interface IModelConfiguration
{
    void Apply( ODataModelBuilder builder, ApiVersion apiVersion, string routePrefix );
}
 ```

The implementation of a model configuration can provide all variations of a model or they can be spit across multiple
implementations. The applied model does not have to be same across API versions.

Consider the following model:

```c#
public class Person
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
}
```

Let us assume that the OData service for this model has three versions: `1.0`, `2.0`, and `3.0`.  In API version `1.0`,
a person had the properties `Id`, `FirstName`, and `LastName`. In API version `2.0` we introduced the `Email` property.
In API version `3.0` we introduced the `Phone` property. If we implement the entire model configuration in a single
class, it might look like:

```c#
public class PersonModelConfiguration : IModelConfiguration
{
    private void ConfigureV1( ODataModelBuilder builder ) =>
        ConfigureCurrent( builder ).Ignore( p => p.Email ).Ignore( p => p.Phone );

    private void ConfigureV2( ODataModelBuilder builder ) =>
        ConfigureCurrent( builder ).Ignore( p => p.Phone );

    private EntityTypeConfiguration<Person> ConfigureCurrent( ODataModelBuilder builder )
    {
        var person = builder.EntitySet<Person>( "People" ).EntityType;
        person.HasKey( p => p.Id );
        return person;
    }

    public void Apply( ODataModelBuilder builder, ApiVersion apiVersion, string routePrefix )
    {
        switch ( apiVersion.MajorVersion )
        {
            case 1:
                ConfigureV1( builder );
                break;
            case 2:
                ConfigureV2( builder );
                break;
            default:
                ConfigureCurrent( builder );
                break;
        }
    }
}
```

Even through we have a single `Person` class, the EDM associated with the service API version will render the model
according the requested API version.

 **~/people(1)?api-version=1.0**

 ```json
 {
    "id": 1,
    "firstName": "John",
    "lastName": "Doe"
 }
 ```

 **~/people(1)?api-version=2.0**

 ```json
 {
    "id": 1,
    "firstName": "John",
    "lastName": "Doe",
    "email": "john.doe@somewhere.com"
 }
 ```

 **~/people(1)?api-version=3.0**

 ```json
 {
    "id": 1,
    "firstName": "John",
    "lastName": "Doe",
    "email": "john.doe@somewhere.com",
    "phone": "555-555-5555"
 }
 ```

[VersionedODataModelBuilder]: model-builder.md