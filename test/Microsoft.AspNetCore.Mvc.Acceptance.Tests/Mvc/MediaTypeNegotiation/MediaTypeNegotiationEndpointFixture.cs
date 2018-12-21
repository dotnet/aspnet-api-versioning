namespace Microsoft.AspNetCore.Mvc.MediaTypeNegotiation
{
    public class MediaTypeNegotiationEndpointFixture : MediaTypeNegotiationFixture
    {
        public MediaTypeNegotiationEndpointFixture() => EnableEndpointRouting = true;
    }
}