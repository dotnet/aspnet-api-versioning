// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0060

namespace Asp.Versioning.Simulators;

using System.Web.Http;

[RoutePrefix( "api/test" )]
public sealed class AttributeRoutedTestController : ApiController
{
    [Route]
    public Task<string> Get() => Task.FromResult( "Test" );

    [Route( "{id}" )]
    public Task<string> Get( string id ) => Task.FromResult( "Test" );
}