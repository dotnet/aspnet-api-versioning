#if WEBAPI
namespace Microsoft.Web.Http.ByNamespace
#else
namespace Microsoft.AspNetCore.Mvc.ByNamespace
#endif
{
    using System;
    using Xunit;

    [CollectionDefinition( nameof( AgreementsCollection ) )]
    public class AgreementsCollection : ICollectionFixture<AgreementsFixture>
    {
    }
}