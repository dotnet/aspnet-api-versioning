namespace ApiVersioning.Examples;

using Asp.Versioning;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using System.Web.Http;
using System.Web.Http.Description;

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
    [HttpGet]
    [ResponseType( typeof( double ) )]
    [ODataRoute( "GetSalesTaxRate(PostalCode={postalCode})" )]
    public IHttpActionResult GetSalesTaxRate( int postalCode ) => Ok( 5.6 );
}