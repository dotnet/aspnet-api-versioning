namespace Microsoft.AspNetCore.Mvc.Basic
{
    using System;
    using Xunit;

    [CollectionDefinition( nameof( BasicEndpointCollection ) )]
    public sealed class BasicEndpointCollection : ICollectionFixture<BasicEndpointFixture>
    {
    }
}