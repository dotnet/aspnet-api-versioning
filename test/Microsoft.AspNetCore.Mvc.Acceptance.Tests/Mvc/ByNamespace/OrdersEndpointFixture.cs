namespace Microsoft.AspNetCore.Mvc.ByNamespace
{
    public class OrdersEndpointFixture : OrdersFixture
    {
        public OrdersEndpointFixture() => EnableEndpointRouting = true;
    }
}