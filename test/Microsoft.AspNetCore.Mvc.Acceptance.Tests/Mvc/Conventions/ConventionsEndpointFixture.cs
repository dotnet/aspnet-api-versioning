namespace Microsoft.AspNetCore.Mvc.Conventions
{
    public class ConventionsEndpointFixture : ConventionsFixture
    {
        public ConventionsEndpointFixture() => EnableEndpointRouting = true;
    }
}