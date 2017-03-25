namespace Microsoft.Web.Http.Description.Simulators
{
    using System.Web.Http;

    public class ValuesController : ApiController
    {
        public IHttpActionResult Get() => Ok();
    }
}