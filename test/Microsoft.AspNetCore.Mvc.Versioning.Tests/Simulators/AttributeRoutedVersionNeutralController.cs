namespace Microsoft.AspNetCore.Mvc.Simulators
{
    using System;
    using System.Threading.Tasks;

    [ApiVersionNeutral]
    [Route( "api/attributed-neutral" )]
    public sealed class AttributeRoutedVersionNeutralController : Controller
    {
        public Task<string> Get() => Task.FromResult( "Test" );
    }
}
