// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData.Advanced;

[Collection( "OData" + nameof( AdvancedTestCollection ) )]
public abstract class AdvancedAcceptanceTest : ODataAcceptanceTest
{
    protected AdvancedAcceptanceTest( ODataFixture fixture ) : base( fixture ) { }
}