## Convention Model

The convention model relies on _Model Bound_ settings via the fluent API of the `ODataModelBuilder`and the
`EnableQueryAttribute`. The `EnableQueryAttribute` indicates API-specific options that might be too restrictive or
nonapplicable to specific models. Consider the following model and controller definitions.

```c#
public class Person
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
}

public class PersonModelConfiguration : IModelConfiguration
{
    public void Apply( ODataModelBuilder builder, ApiVersion apiVersion, string routePrefix )
    {
        var person = builder.EntitySet<Person>( "People" ).EntityType;

        person.HasKey( p => p.Id );

        // configure model bound conventions
        person.Select().OrderBy( "firstName", "lastName" );

        if ( apiVersion < ApiVersions.V3 )
        {
            person.Ignore( p => p.Phone );
        }

        if ( apiVersion <= ApiVersions.V1 )
        {
            person.Ignore( p => p.Email );
        }

        if ( apiVersion > ApiVersions.V1 )
        {
            var function = person.Collection.Function( "NewHires" );

            function.Parameter<DateTime>( "Since" );
            function.ReturnsFromEntitySet<Person>( "People" );
        }

        if ( apiVersion > ApiVersions.V2 )
        {
            person.Action( "Promote" ).Parameter<string>( "title" );
        }
    }
}
````