namespace Microsoft.AspNetCore.Mvc.Basic
{
    public class OverlappingRouteTemplateEndpointFixture : OverlappingRouteTemplateFixture
    {
        public OverlappingRouteTemplateEndpointFixture() => EnableEndpointRouting = true;
    }
}