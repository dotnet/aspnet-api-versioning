#if WEBAPI
namespace Microsoft.Web.Http.Conventions
#else
namespace Microsoft.AspNetCore.Mvc.Conventions
#endif
{
    using System;
    using Xunit;

    [CollectionDefinition( nameof( ConventionsCollection ) )]
    public class ConventionsCollection : ICollectionFixture<ConventionsFixture>
    {
    }
}