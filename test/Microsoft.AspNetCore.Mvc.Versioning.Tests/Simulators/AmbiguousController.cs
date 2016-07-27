namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using System;
    using System.Threading.Tasks;

    [ApiVersion( "1.0" )]
    public sealed class AmbiguousController : Controller
    {
        public Task<string> Get() => Task.FromResult( "Test" );
    }
}
