namespace Microsoft.AspNetCore.Mvc.ByNamespace
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Routing;

    public class AgreementsEndpointFixture : AgreementsFixture
    {
        public AgreementsEndpointFixture() => EnableEndpointRouting = true;

#if !NET461
        protected override void OnConfigureEndpoints( IEndpointRouteBuilder routeBuilder )
        {
            routeBuilder.MapControllerRoute( "VersionedQueryString", "api/{controller}/{accountId}/{action=Get}" );
            routeBuilder.MapControllerRoute( "VersionedUrl", "v{version:apiVersion}/{controller}/{accountId}/{action=Get}" );
        }
#endif
    }
}