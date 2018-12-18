namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using System;
    using System.Threading.Tasks;

    [ApiController]
    [ApiVersionNeutral]
    [ControllerName( "Ambiguous" )]
    public sealed class AmbiguousNeutralController : ControllerBase
    {
        [HttpGet]
        public Task<string> Get() => Task.FromResult( "Test" );
    }
}