namespace Microsoft.AspNetCore.OData.Advanced
{
    using System;
    using Xunit;

    [CollectionDefinition( nameof( AdvancedODataEndpointCollection ) )]
    public sealed class AdvancedODataEndpointCollection : ICollectionFixture<AdvancedEndpointFixture>
    {
    }
}
