#if WEBAPI
namespace Microsoft.AspNet.OData.Conventions
#else
namespace Microsoft.AspNetCore.OData.Conventions
#endif
{
    using System;
    using Xunit;

    [CollectionDefinition( nameof( ConventionsODataCollection ) )]
    public class ConventionsODataCollection : ICollectionFixture<ConventionsFixture>
    {
    }
}