namespace ApiVersioning.Examples;

using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using static Microsoft.AspNetCore.Http.StatusCodes;

/// <summary>
/// Provides unbound, utility functions.
/// </summary>
[ApiVersionNeutral]
public class FunctionsController : ODataController
{
    /// <summary>
    /// Get Sales Tax
    /// </summary>
    /// <description>Gets the sales tax for a postal code.</description>
    /// <param name="postalCode">The postal code to get the sales tax for.</param>
    /// <returns>The sales tax rate for the postal code.</returns>
    /// <response code="200">The sales tax was successfully retrieved.</response>
    /// <response code="404">The postal code was not found.</response>
    [HttpGet( "api/GetSalesTaxRate(PostalCode={postalCode})" )]
    [ProducesResponseType( typeof( double ), Status200OK )]
    [ProducesResponseType( Status404NotFound )]
    public IActionResult GetSalesTaxRate( int postalCode ) => Ok( 5.6 );
}