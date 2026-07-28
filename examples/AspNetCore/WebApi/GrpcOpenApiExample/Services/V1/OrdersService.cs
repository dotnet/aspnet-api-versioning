namespace ApiVersioning.Examples.Services.V1;

using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

/// <summary>
/// Represents a gRPC orders service
/// </summary>
public class OrdersService : Orders.OrdersBase
{
    /// <summary>
    /// Get Order
    /// </summary>
    /// <description>Gets a single order</description>
    /// <param name="request">The request message</param>
    /// <param name="context">The current call context</param>
    /// <returns>The requested order</returns>
    /// <response code="200">The order was successfully retrieved</response>
    /// <response code="404">The order does not exist</response>
    public override Task<OrderReply> GetOrder( OrderIdRequest request, ServerCallContext context ) =>
        Task.FromResult(
            new OrderReply()
            {
                Order = new()
                {
                    Id = request.Id,
                    Customer = "John Doe",
                    CreatedDate = DateTime.UtcNow.ToTimestamp(),
                    EffectiveDate = DateTime.UtcNow.AddDays( 7 ).ToTimestamp(),
                    LineItems =
                    {
                        new LineItem[]
                        {
                            new() { Number = 1, Quantity = 1, UnitPrice = 2.0, Description = "Dry erase wipes" },
                            new() { Number = 2, Quantity = 1, UnitPrice = 3.5, Description = "Dry erase eraser" },
                            new() { Number = 3, Quantity = 1, UnitPrice = 5.0, Description = "Dry erase markers" },
                        }
                    }
                }
            } );

    /// <summary>
    /// Place Order
    /// </summary>
    /// <description>Places a new order</description>
    /// <param name="request">The request message</param>
    /// <param name="context">The current call context</param>
    /// <returns>The created order</returns>
    /// <response code="200">The order was successfully placed</response>
    /// <response code="400">The order is invalid</response>
    public override Task<OrderReply> PlaceOrder( OrderRequest request, ServerCallContext context )
    {
        var order = request.Order;
        order.Id = 42;
        return Task.FromResult( new OrderReply() { Order = order } );
    }

    /// <summary>
    /// Update Order
    /// </summary>
    /// <description>Updates an existing order</description>
    /// <param name="request">The request message</param>
    /// <param name="context">The current call context</param>
    /// <returns>The created order</returns>
    /// <response code="200">The order was successfully updated</response>
    /// <response code="400">The order is invalid</response>
    /// <response code="404">The order does not exist</response>
    public override Task<Empty> UpdateOrder( OrderRequest request, ServerCallContext context ) =>
        Task.FromResult( new Empty() );

    /// <summary>
    /// Cancel Order
    /// </summary>
    /// <description>Cancels an order</description>
    /// <param name="request">The request message</param>
    /// <param name="context">The current call context</param>
    /// <returns>None</returns>
    /// <response code="200">The order was successfully canceled</response>
    /// <response code="404">The order does not exist</response>
    public override Task<Empty> CancelOrder( OrderIdRequest request, ServerCallContext context ) =>
        Task.FromResult( new Empty() );
}