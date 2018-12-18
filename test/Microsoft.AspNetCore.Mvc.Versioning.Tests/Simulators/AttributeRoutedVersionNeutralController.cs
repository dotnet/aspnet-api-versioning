namespace Microsoft.AspNetCore.Mvc.Simulators
{
    using System;
    using System.Threading.Tasks;

    [ApiController]
    [ApiVersionNeutral]
    [Route( "api/attributed-neutral" )]
    public sealed class AttributeRoutedVersionNeutralController : ControllerBase
    {
        [HttpGet]
        public Task<string> Get() => Task.FromResult( "Test" );
    }
}