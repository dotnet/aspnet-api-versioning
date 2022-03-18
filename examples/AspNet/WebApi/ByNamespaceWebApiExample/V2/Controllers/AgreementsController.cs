namespace ApiVersioning.Examples.V2.Controllers;

using ApiVersioning.Examples.V2.Models;
using Asp.Versioning;
using System.Web.Http;

public class AgreementsController : ApiController
{
    // GET ~/v2/agreements/{accountId}
    // GET ~/agreements/{accountId}?api-version=2.0
    public IHttpActionResult Get( string accountId, ApiVersion apiVersion ) =>
        Ok( new Agreement( GetType().FullName, accountId, apiVersion.ToString() ) );
}