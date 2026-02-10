namespace ApiVersioning.Examples.Services;

using Asp.Versioning;

/// <summary>
/// Provides the endpoint extensions for the People service.
/// </summary>
public static class People
{
    extension( PeopleApiBuilder apiBuilder )
    {
        /// <summary>
        /// Maps the People APIs for <c>1.0</c>.
        /// </summary>
        /// <returns>The next builder.</returns>
        public VersionedApiBuilder<Models.V2.Person> ToV1()
        {
            var people = new VersionedApiBuilder<Models.V1.Person>( apiBuilder.Endpoints );
            var builder = people.Endpoints;
            var api = builder.MapGroup( "/api/v{version:apiVersion}/people" )
                             .HasDeprecatedApiVersion( 0.9 )
                             .HasApiVersion( 1.0 );

            api.MapGet( "/{id:int}", V1.Get )
               .Produces( 200, people.ModelType )
               .Produces( 404 );

            return new( builder );
        }
    }

    extension( VersionedApiBuilder<Models.V2.Person> people )
    {
        /// <summary>
        /// Maps the People APIs for <c>2.0</c>.
        /// </summary>
        /// <returns>The next builder.</returns>
        public VersionedApiBuilder<Models.V3.Person> ToV2()
        {
            var builder = people.Endpoints;
            var api = builder.MapGroup( "/api/v{version:apiVersion}/people" )
                             .HasApiVersion( 2.0 );

            api.MapGet( "/", V2.GetAll )
               .Produces( 200, people.EnumerableModelType );

            api.MapGet( "/{id:int}", V2.GetById )
               .WithSummary( "Get Person" )
               .WithDescription( "Gets a single person." )
               .Produces( 200, people.ModelType )
               .Produces( 404 );

            return new( builder );
        }
    }

    extension( VersionedApiBuilder<Models.V3.Person> people )
    {
        /// <summary>
        /// Maps the Person APIs for <c>3.0</c>.
        /// </summary>
        public void ToV3()
        {
            var builder = people.Endpoints;
            var api = builder.MapGroup( "/api/v{version:apiVersion}/people" )
                             .HasApiVersion( 3.0 );

            api.MapGet( "/", V3.GetAll )
               .Produces( 200, people.EnumerableModelType );

            api.MapGet( "/{id:int}", V3.GetById )
               .Produces( 200, people.ModelType )
               .Produces( 404 );

            api.MapPost( "/", V3.Post )
               .Accepts<Models.V3.Person>( "application/json" )
               .Produces<Models.V3.Person>( 201 )
               .Produces( 400 );
        }
    }

    /// <summary>
    /// Represents the 1.0 People service.
    /// </summary>
    public static class V1
    {
        /// <summary>
        /// Get Person
        /// </summary>
        /// <description>Gets a single person.</description>
        /// <param name="id">The requested person identifier.</param>
        /// <returns>The requested person.</returns>
        /// <response code="200">The person was successfully retrieved.</response>
        /// <response code="404">The person does not exist.</response>
        public static Models.V1.Person Get( int id ) => new()
        {
            Id = id,
            FirstName = "John",
            LastName = "Doe",
        };
    }

    /// <summary>
    /// Represents the 2.0 People service.
    /// </summary>
    public static class V2
    {
        /// <summary>
        /// Get People
        /// </summary>
        /// <description>Gets all people.</description>
        /// <returns>All available people.</returns>
        /// <response code="200">The successfully retrieved people.</response>
        public static Models.V2.Person[] GetAll() =>
        [
            new()
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@somewhere.com",
            },
            new()
            {
                Id = 2,
                FirstName = "Bob",
                LastName = "Smith",
                Email = "bob.smith@somewhere.com",
            },
            new()
            {
                Id = 3,
                FirstName = "Jane",
                LastName = "Doe",
                Email = "jane.doe@somewhere.com",
            },
        ];

        /// <summary>
        /// Get Person
        /// </summary>
        /// <description>Gets a single person.</description>
        /// <param name="id">The requested person identifier.</param>
        /// <returns>The requested person.</returns>
        /// <response code="200">The person was successfully retrieved.</response>
        /// <response code="404">The person does not exist.</response>
        public static Models.V2.Person GetById( int id ) => new()
        {
            Id = id,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@somewhere.com",
        };
    }

    /// <summary>
    /// Represents the 3.0 People service.
    /// </summary>
    public static class V3
    {
        /// <summary>
        /// Get People
        /// </summary>
        /// <description>Gets all people.</description>
        /// <returns>All available people.</returns>
        /// <response code="200">The successfully retrieved people.</response>
        public static Models.V3.Person[] GetAll() =>
        [
            new()
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@somewhere.com",
                Phone = "555-987-1234",
            },
            new()
            {
                Id = 2,
                FirstName = "Bob",
                LastName = "Smith",
                Email = "bob.smith@somewhere.com",
                Phone = "555-654-4321",
            },
            new()
            {
                Id = 3,
                FirstName = "Jane",
                LastName = "Doe",
                Email = "jane.doe@somewhere.com",
                Phone = "555-789-3456",
            },
        ];

        /// <summary>
        /// Get Person
        /// </summary>
        /// <description>Gets a single person.</description>
        /// <param name="id">The requested person identifier.</param>
        /// <returns>The requested person.</returns>
        /// <response code="200">The person was successfully retrieved.</response>
        /// <response code="404">The person does not exist.</response>
        public static Models.V3.Person GetById( int id ) => new()
        {
            Id = id,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@somewhere.com",
            Phone = "555-987-1234",
        };

        /// <summary>
        /// Add Person
        /// </summary>
        /// <description>Adds a new person.</description>
        /// <param name="request"></param>
        /// <param name="version"></param>
        /// <param name="person">The person to create.</param>
        /// <returns>The created person.</returns>
        /// <response code="201">The person was successfully created.</response>
        /// <response code="400">The person was invalid.</response>
        public static IResult Post( HttpRequest request, ApiVersion version, Models.V3.Person person )
        {
            person.Id = 42;
            var scheme = request.Scheme;
            var host = request.Host;
            var location = new Uri( $"{scheme}{Uri.SchemeDelimiter}{host}/v{version}/api/people/{person.Id}" );
            return Results.Created( location, person );
        }
    }
}