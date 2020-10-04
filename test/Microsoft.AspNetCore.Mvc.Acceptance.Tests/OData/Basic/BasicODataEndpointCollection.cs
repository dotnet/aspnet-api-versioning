namespace Microsoft.AspNetCore.OData.Basic
{
    using System;
    using Xunit;

    [CollectionDefinition( nameof( BasicODataEndpointCollection ) )]
    public sealed class BasicODataEndpointCollection : ICollectionFixture<BasicEndpointFixture>
    {
    }
}