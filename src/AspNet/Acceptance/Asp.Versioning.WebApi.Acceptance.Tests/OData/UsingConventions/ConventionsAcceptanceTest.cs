// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData.UsingConventions;

[Collection( "OData" + nameof( ConventionsTestCollection ) )]
public abstract class ConventionsAcceptanceTest : ODataAcceptanceTest
{
    protected ConventionsAcceptanceTest( ConventionsFixture fixture ) : base( fixture ) { }
}