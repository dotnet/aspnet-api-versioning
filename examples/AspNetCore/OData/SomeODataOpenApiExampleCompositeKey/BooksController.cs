namespace ApiVersioning.Examples;

using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.AspNetCore.Http.StatusCodes;

/// <summary>
/// Represents a RESTful service of books.
/// </summary>
[Asp.Versioning.ApiVersion( "1.0" )]
[Asp.Versioning.ApiVersion( "2.0" )]
public class BooksController : ODataController
{
    private static readonly Book[] books = new Book[]
    {
        new() { IdFirst = "9781847490599",IdSecond = "9781847490599", Title = "Anna Karenina", Author = "Leo Tolstoy", Published = 1878 },
        new() { IdFirst = "9780198800545",IdSecond = "9780198800545", Title = "War and Peace", Author = "Leo Tolstoy", Published = 1869 },
        new() { IdFirst = "9780684801520",IdSecond = "9780684801520", Title = "The Great Gatsby", Author = "F. Scott Fitzgerald", Published = 1925 },
        new() { IdFirst = "9780486280615",IdSecond = "9780486280615", Title = "The Adventures of Huckleberry Finn", Author = "Mark Twain", Published = 1884 },
        new() { IdFirst = "9780140430820",IdSecond = "9780140430820", Title = "Moby Dick", Author = "Herman Melville", Published = 1851 },
        new() { IdFirst = "9780060934347",IdSecond = "9780060934347", Title = "Don Quixote", Author = "Miguel de Cervantes", Published = 1605 },
    };

    /// <summary>
    /// Gets all books.
    /// </summary>
    /// <returns>All available books.</returns>
    /// <response code="200">The successfully retrieved books.</response>
    [Produces( "application/json" )]
    [ProducesResponseType( typeof( IEnumerable<Book> ), Status200OK )]
    public IActionResult Get() =>
        Ok( books.AsQueryable() );

    /// <summary>
    /// Gets a single book.
    /// </summary>
    /// <param name="key">The requested book identifier.</param>
    /// <returns>The requested book.</returns>
    /// <response code="200">The book was successfully retrieved.</response>
    /// <response code="404">The book does not exist.</response>
    //[Produces("application/json")]
    //[ProducesResponseType(typeof(Book), Status200OK)]
    //[ProducesResponseType(Status404NotFound)]
    //public IActionResult Get(string key)
    //{
    //    var book = books.FirstOrDefault(book => book.Id == key);

    //    if (book == null)
    //    {
    //        return NotFound();
    //    }

    //    return Ok(book);
    //}

    /// <summary>
    /// Gets a single book.
    /// </summary>
    [Produces( "application/json" )]
    [ProducesResponseType( typeof( Book ), Status200OK )]
    [ProducesResponseType( Status404NotFound )]
    public IActionResult Get( string keyidFirst, string keyidSecond )
    {
        var book = books.FirstOrDefault( book => book.IdFirst == keyidFirst && book.IdSecond == keyidSecond );

        if ( book == null )
        {
            return NotFound();
        }

        return Ok( book );
    }
}