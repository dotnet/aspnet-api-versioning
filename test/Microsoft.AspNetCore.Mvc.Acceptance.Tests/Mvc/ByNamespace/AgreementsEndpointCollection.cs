namespace Microsoft.AspNetCore.Mvc.ByNamespace
{
    using System;
    using Xunit;

    [CollectionDefinition( nameof( AgreementsEndpointCollection ) )]
    public class AgreementsEndpointCollection : ICollectionFixture<AgreementsEndpointFixture>
    {
    }
}
