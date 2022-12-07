// Copyright (c) .NET Foundation and contributors. All rights reserved.


namespace Asp.Versioning.Simulators;

using System.Web.Http;

[AdvertiseApiVersions( "1.0" )]
[ApiVersion( "2.0" )]
[ApiVersion( "3.0" )]
[RoutePrefix( "api/test" )]
public sealed class AttributeRoutedTest2Controller : ApiController
{
    [Route]
    public Task<string> Get() => Task.FromResult( "Test" );

    [Route]
    [MapToApiVersion( "3.0" )]
    public Task<string> GetV3() => Task.FromResult( "Test" );
}