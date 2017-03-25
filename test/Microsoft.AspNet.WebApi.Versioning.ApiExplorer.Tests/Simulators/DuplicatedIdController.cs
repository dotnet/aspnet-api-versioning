namespace Microsoft.Web.Http.Description.Simulators
{
    using Models;
    using System.Web.Http;

    public class DuplicatedIdController : ApiController
    {
        public void Get( [FromUri] ClassWithId objectWithId ) { }
    }
}