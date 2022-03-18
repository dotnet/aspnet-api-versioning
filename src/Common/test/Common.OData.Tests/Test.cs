// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using Microsoft.OData.Edm;
#if NETFRAMEWORK
using Microsoft.AspNet.OData.Builder;
#else
using Microsoft.OData.ModelBuilder;
#endif

internal static class Test
{
    static Test()
    {
        var builder = new ODataModelBuilder();
        var tests = builder.EntitySet<TestEntity>( "Tests" ).EntityType;
        var neutralTests = builder.EntitySet<TestNeutralEntity>( "NeutralTests" ).EntityType;

        tests.HasKey( t => t.Id );
        neutralTests.HasKey( t => t.Id );
        Model = builder.GetEdmModel();
    }

    internal static IEdmModel Model { get; }

    internal static IEdmModel EmptyModel { get; } = new ODataModelBuilder().GetEdmModel();
}