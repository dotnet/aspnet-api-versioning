#if WEBAPI
namespace Microsoft.AspNet.OData.Basic
#else
namespace Microsoft.AspNetCore.OData.Basic
#endif
{
    using System;
    using Xunit;

    [CollectionDefinition( nameof( BasicODataCollection ) )]
    public sealed class BasicODataCollection : ICollectionFixture<BasicFixture>
    {
    }
}