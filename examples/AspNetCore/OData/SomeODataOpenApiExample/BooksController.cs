namespace ApiVersioning.Examples;

using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using static Microsoft.AspNetCore.Http.StatusCodes;

/// <summary>
/// Represents a RESTful service of books.
/// </summary>
[ApiVersion( 1.0 )]
[ApiController]
[Route( "api/[controller]" )]
public class BooksController : ControllerBase
{
    private static readonly Book[] books =
    [
        new() { Id = "9781847490599", Title = "Anna Karenina", Author = "Leo Tolstoy", Published = 1878 },
        new() { Id = "9780198800545", Title = "War and Peace", Author = "Leo Tolstoy", Published = 1869 },
        new() { Id = "9780684801520", Title = "The Great Gatsby", Author = "F. Scott Fitzgerald", Published = 1925 },
        new() { Id = "9780486280615", Title = "The Adventures of Huckleberry Finn", Author = "Mark Twain", Published = 1884 },
        new() { Id = "9780140430820", Title = "Moby Dick", Author = "Herman Melville", Published = 1851 },
        new() { Id = "9780060934347", Title = "Don Quixote", Author = "Miguel de Cervantes", Published = 1605 },
    ];

    /// <summary>
    /// Gets all books.
    /// </summary>
    /// <param name="options">The current OData query options.</param>
    /// <returns>All available books.</returns>
    /// <response code="200">The successfully retrieved books.</response>
    [HttpGet]
    [Produces( "application/json" )]
    [ProducesResponseType( typeof( IEnumerable<Book> ), Status200OK )]
    public IActionResult Get( ODataQueryOptions<Book> options ) =>
        Ok( options.ApplyTo( books.AsQueryable() ) );

    /// <summary>
    /// Gets a single book.
    /// </summary>
    /// <param name="id">The requested book identifier.</param>
    /// <param name="options">The current OData query options.</param>
    /// <returns>The requested book.</returns>
    /// <response code="200">The book was successfully retrieved.</response>
    /// <response code="404">The book does not exist.</response>
    [HttpGet( "{id}" )]
    [Produces( "application/json" )]
    [ProducesResponseType( typeof( Book ), Status200OK )]
    [ProducesResponseType( Status404NotFound )]
    public IActionResult Get( string id, ODataQueryOptions<Book> options )
    {
        var book = books.FirstOrDefault( book => book.Id == id );

        if ( book == null )
        {
            return NotFound();
        }

        return Ok( options.ApplyTo( book, new ODataQuerySettings(), default ) );
    }
}