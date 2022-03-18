namespace ApiVersioning.Examples.V1.Controllers;

using ApiVersioning.Examples.V1.Models;
using Asp.Versioning;
using System.Web.Http;

public class AgreementsController : ApiController
{
    // GET ~/v1/agreements/{accountId}
    // GET ~/agreements/{accountId}?api-version=1.0
    public IHttpActionResult Get( string accountId, ApiVersion apiVersion ) =>
        Ok( new Agreement( GetType().FullName, accountId, apiVersion.ToString() ) );
}