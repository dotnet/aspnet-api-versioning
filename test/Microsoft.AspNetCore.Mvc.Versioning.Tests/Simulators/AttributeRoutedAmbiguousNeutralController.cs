﻿namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using System;
    using System.Threading.Tasks;

    [ApiController]
    [ApiVersionNeutral]
    [Route( "api/attributed-ambiguous" )]
    public sealed class AttributeRoutedAmbiguousNeutralController : ControllerBase
    {
        [HttpGet]
        public Task<string> Get() => Task.FromResult( "Test" );
    }
}