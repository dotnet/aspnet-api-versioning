namespace ApiVersioning.Examples.V3.Controllers;

using ApiVersioning.Examples.V3.Models;
using Asp.Versioning;
using System.Web.Http;

public class AgreementsController : ApiController
{
    // GET ~/v3/agreements/{accountId}
    // GET ~/agreements/{accountId}?api-version=3.0
    public IHttpActionResult Get( string accountId, ApiVersion apiVersion ) =>
        Ok( new Agreement( GetType().FullName, accountId, apiVersion.ToString() ) );
}