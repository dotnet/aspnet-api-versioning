namespace Microsoft.Web.Http.Simulators
{
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.Controllers;

    public sealed class ConventionsController : ApiController
    {
        public string Get() => "Test (1.0)";

        public string Get( int id ) => $"Test {id} (1.0)";

        public string GetV2() => "Test (2.0)";

        public string GetV2( int id ) => $"Test {id} (2.0)";
    }
}