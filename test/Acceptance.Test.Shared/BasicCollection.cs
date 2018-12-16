#if WEBAPI
namespace Microsoft.Web.Http.Basic
#else
namespace Microsoft.AspNetCore.Mvc.Basic
#endif
{
    using System;
    using Xunit;

    [CollectionDefinition( nameof( BasicCollection ) )]
    public sealed class BasicCollection : ICollectionFixture<BasicFixture>
    {
    }
}