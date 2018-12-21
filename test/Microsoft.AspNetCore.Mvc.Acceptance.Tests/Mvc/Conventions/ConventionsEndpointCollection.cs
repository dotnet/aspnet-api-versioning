namespace Microsoft.AspNetCore.Mvc.Conventions
{
    using System;
    using Xunit;

    [CollectionDefinition( nameof( ConventionsEndpointCollection ) )]
    public class ConventionsEndpointCollection : ICollectionFixture<ConventionsEndpointFixture>
    {
    }
}
