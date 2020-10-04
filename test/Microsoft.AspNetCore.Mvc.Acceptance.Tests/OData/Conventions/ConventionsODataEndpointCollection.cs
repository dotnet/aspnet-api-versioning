namespace Microsoft.AspNetCore.OData.Conventions
{
    using System;
    using Xunit;

    [CollectionDefinition( nameof( ConventionsODataEndpointCollection ) )]
    public sealed class ConventionsODataEndpointCollection : ICollectionFixture<ConventionsEndpointFixture>
    {
    }
}