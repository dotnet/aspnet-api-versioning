namespace ApiVersioning.Examples.Services;

/// <summary>
/// Represents an API builder.
/// </summary>
public static class ApiBuilder
{
    extension( IEndpointRouteBuilder app )
    {
        /// <summary>
        /// Creates and returns an API builder for the Orders service.
        /// </summary>
        /// <returns>A new <see cref="OrdersApiBuilder">Orders API builder</see>.</returns>
        public OrdersApiBuilder MapOrders() => new( app.NewVersionedApi( "Orders" ) );

        /// <summary>
        /// Creates and returns an API builder for the Orders service.
        /// </summary>
        /// <returns>A new <see cref="OrdersApiBuilder">Orders API builder</see>.</returns>
        public PeopleApiBuilder MapPeople() => new( app.NewVersionedApi( "People" ) );
    }
}
