namespace ApiVersioning.Examples.Services.V3;

using Grpc.Core;

/// <summary>
/// Represents a gRPC people service
/// </summary>
public class PeopleService : People.PeopleBase
{
    /// <summary>
    /// Get People
    /// </summary>
    /// <description>Gets all people</description>
    /// <param name="request">The request message</param>
    /// <param name="context">The current call context</param>
    /// <returns>All available people</returns>
    /// <response code="200">The successfully retrieved people</response>
    public override Task<PeopleReply> GetPeople( PeopleRequest request, ServerCallContext context ) =>
        Task.FromResult(
            new PeopleReply()
            {
                People =
                {
                    new Person[]
                    {
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
                    }
                }
            } );

    /// <summary>
    /// Get Person
    /// </summary>
    /// <description>Gets a single person</description>
    /// <param name="request">The request message</param>
    /// <param name="context">The current call context</param>
    /// <returns>The requested person</returns>
    /// <response code="200">The person was successfully retrieved</response>
    /// <response code="404">The person does not exist</response>
    public override Task<PeopleReply> GetPerson( PeopleIdRequest request, ServerCallContext context ) =>
        Task.FromResult( new PeopleReply()
        {
            Person = new()
            {
                Id = request.Id,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@somewhere.com",
                Phone = "555-987-1234",
            }
        } );

    /// <summary>
    /// Add Person
    /// </summary>
    /// <description>Adds a new person</description>
    /// <param name="request">The request message</param>
    /// <param name="context">The current call context</param>
    /// <returns>The created person</returns>
    /// <response code="201">The person was successfully created</response>
    /// <response code="400">The person was invalid</response>
    public override Task<PeopleReply> AddPerson( PersonRequest request, ServerCallContext context )
    {
        var person = request.Person;
        person.Id = 42;
        return Task.FromResult( new PeopleReply() { Person = person } );
    }

}