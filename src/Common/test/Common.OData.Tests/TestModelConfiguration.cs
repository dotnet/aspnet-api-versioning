// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using Asp.Versioning.OData;
#if NETFRAMEWORK
using Microsoft.AspNet.OData.Builder;
#else
using Microsoft.OData.ModelBuilder;
#endif

public class TestModelConfiguration : IModelConfiguration
{
    public void Apply( ODataModelBuilder builder, ApiVersion apiVersion, string routePrefix )
    {
#if NETFRAMEWORK
        if ( builder is null )
        {
            throw new ArgumentNullException( nameof( builder ) );
        }
#else
        ArgumentNullException.ThrowIfNull( builder );
#endif

        var tests = builder.EntitySet<TestEntity>( "Tests" ).EntityType;
        var neutralTests = builder.EntitySet<TestNeutralEntity>( "NeutralTests" ).EntityType;

        tests.HasKey( t => t.Id );
        neutralTests.HasKey( t => t.Id );
    }
}