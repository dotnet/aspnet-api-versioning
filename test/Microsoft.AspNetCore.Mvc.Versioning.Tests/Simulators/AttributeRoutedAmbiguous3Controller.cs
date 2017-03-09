namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using System;
    using System.Threading.Tasks;

    [ApiVersion( "1.0" )]
    [Route( "api/attributed/ambiguous" )]
    public sealed class AttributeRoutedAmbiguous3Controller : Controller
    {
        [HttpGet]
        public Task<string> Get() => Task.FromResult( "Test" );
    }
}