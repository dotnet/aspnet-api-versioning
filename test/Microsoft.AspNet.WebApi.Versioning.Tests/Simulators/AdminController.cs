namespace Microsoft.Web.Http.Simulators
{
    using System;
    using System.Threading.Tasks;
    using System.Web.Http;

    [ApiVersionNeutral]
    public class AdminController : ApiController
    {
        [Route( "admin" )]
        public IHttpActionResult Get() => Ok();

        [HttpPost]
        public IHttpActionResult SeedData() => Ok();

        [HttpPost]
        public IHttpActionResult MarkAsTest() => Ok();

        [HttpPost]
        [Route( "admin/inject" )]
        public IHttpActionResult Inject() => Ok();
    }
}
