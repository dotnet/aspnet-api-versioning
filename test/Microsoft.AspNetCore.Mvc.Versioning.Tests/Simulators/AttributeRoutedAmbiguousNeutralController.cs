namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using System;
    using System.Threading.Tasks;

    [ApiVersionNeutral]
    [Route( "api/attributed-ambiguous" )]
    public sealed class AttributeRoutedAmbiguousNeutralController : Controller
    {
        [HttpGet]
        public Task<string> Get() => Task.FromResult( "Test" );
    }
}