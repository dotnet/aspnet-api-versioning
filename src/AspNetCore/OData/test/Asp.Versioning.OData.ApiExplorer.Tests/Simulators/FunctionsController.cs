// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0060 // Remove unused parameter

namespace Asp.Versioning.Simulators;

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
    /// Gets the sales tax for a postal code.
    /// </summary>
    /// <param name="postalCode">The postal code to get the sales tax for.</param>
    /// <returns>The sales tax rate for the postal code.</returns>
    [HttpGet( "api/GetSalesTaxRate(PostalCode={postalCode})" )]
    [ProducesResponseType( typeof( double ), Status200OK )]
    public IActionResult GetSalesTaxRate( int postalCode ) => Ok( 5.6 );
}