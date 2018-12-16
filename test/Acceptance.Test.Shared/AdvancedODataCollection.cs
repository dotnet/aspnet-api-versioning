#if WEBAPI
namespace Microsoft.AspNet.OData.Advanced
#else
namespace Microsoft.AspNetCore.OData.Advanced
#endif
{
    using System;
    using Xunit;

    [CollectionDefinition( nameof( AdvancedODataCollection ) )]
    public sealed class AdvancedODataCollection : ICollectionFixture<AdvancedFixture>
    {
    }
}