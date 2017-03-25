namespace Microsoft.Web.Http.Description.Simulators
{
    using System.Web.Http;
    using System.Web.Http.Description;

    public class ApiExplorerValuesController : ApiController
    {
        public void Get() { }

        [ApiExplorerSettings( IgnoreApi = true )]
        public void Post() { }
    }
}