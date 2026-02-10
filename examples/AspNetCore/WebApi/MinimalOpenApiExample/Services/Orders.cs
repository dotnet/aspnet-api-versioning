namespace ApiVersioning.Examples.Services;

/// <summary>
/// Provides the endpoint extensions for the Orders service.
/// </summary>
public static class Orders
{
    extension( OrdersApiBuilder apiBuilder )
    {
        /// <summary>
        /// Maps the Orders APIs for <c>1.0</c>.
        /// </summary>
        /// <returns>The next builder.</returns>
        public VersionedApiBuilder<Models.V2.Order> ToV1()
        {
            var orders = new VersionedApiBuilder<Models.V1.Order>( apiBuilder.Endpoints );
            var builder = orders.Endpoints;
            var api = builder.MapGroup( "/api/orders" )
                             .HasDeprecatedApiVersion( 0.9 )
                             .HasApiVersion( 1.0 );

            api.MapGet( "/{id:int}", V1.Get )
               .Produces( 200, orders.ModelType )
               .Produces( 404 );

            api.MapPost( "/", V1.Post )
                .Accepts( orders.ModelType, "application/json" )
                .Produces( 201, orders.ModelType )
                .Produces( 400 )
                .MapToApiVersion( 1.0 );

            api.MapPatch( "/{id:int}", V1.Patch )
               .Accepts( orders.ModelType, "application/json" )
               .Produces( 204 )
               .Produces( 400 )
               .Produces( 404 )
               .MapToApiVersion( 1.0 );

            return new( builder );
        }
    }

    extension( VersionedApiBuilder<Models.V2.Order> orders )
    {
        /// <summary>
        /// Maps the Orders APIs for <c>2.0</c>.
        /// </summary>
        /// <returns>The next builder.</returns>
        public VersionedApiBuilder<Models.V3.Order> ToV2()
        {
            var builder = orders.Endpoints;
            var api = builder.MapGroup( "/api/orders" )
                             .HasApiVersion( 2.0 );

            api.MapGet( "/", V2.GetAll )
               .Produces( 200, orders.EnumerableModelType )
               .Produces( 404 );

            api.MapGet( "/{id:int}", V2.GetById )
               .Produces( 200, orders.ModelType )
               .Produces( 404 );

            api.MapPost( "/", V2.Post )
               .Accepts( orders.ModelType, "application/json" )
               .Produces( 201, orders.ModelType )
               .Produces( 400 );

            api.MapPatch( "/{id:int}", V2.Patch )
               .Accepts( orders.ModelType, "application/json" )
               .Produces( 204 )
               .Produces( 400 )
               .Produces( 404 );

            return new( builder );
        }
    }

    extension( VersionedApiBuilder<Models.V3.Order> orders )
    {
        /// <summary>
        /// Maps the Orders APIs for <c>3.0</c>.
        /// </summary>
        public void ToV3()
        {
            var builder = orders.Endpoints;
            var api = builder.MapGroup( "/api/orders" )
                             .HasApiVersion( 3.0 );

            api.MapGet( "/", V3.GetAll )
               .Produces( 200, orders.EnumerableModelType );

            api.MapGet( "/{id:int}", V3.GetById )
               .Produces( 200, orders.ModelType )
               .Produces( 404 );

            api.MapPost( "/", V3.Post )
               .Accepts( orders.ModelType, "application/json" )
               .Produces( 201 , orders.ModelType )
               .Produces( 400 );

            api.MapDelete( "/{id:int}", V3.Delete )
               .Produces( 204 );
        }
    }

    /// <summary>
    /// Represents the 1.0 Orders service.
    /// </summary>
    public static class V1
    {
        /// <summary>
        /// Get Order
        /// </summary>
        /// <description>Gets a single order.</description>
        /// <param name="id">The requested order identifier.</param>
        /// <returns>The requested order.</returns>
        /// <response code="200">The order was successfully retrieved.</response>
        /// <response code="404">The order does not exist.</response>
        public static Models.V1.Order Get( int id ) => new() { Id = id, Customer = "John Doe" };

        /// <summary>
        /// Place Order
        /// </summary>
        /// <description>Places a new order.</description>
        /// <param name="request"></param>
        /// <param name="order">The order to place.</param>
        /// <returns>The created order.</returns>
        /// <response code="201">The order was successfully placed.</response>
        /// <response code="400">The order is invalid.</response>
        public static IResult Post( HttpRequest request, Models.V1.Order order )
        {
            order.Id = 42;
            var scheme = request.Scheme;
            var host = request.Host;
            var location = new Uri( $"{scheme}{Uri.SchemeDelimiter}{host}/api/orders/{order.Id}" );
            return Results.Created( location, order );
        }

        /// <summary>
        /// Update Order
        /// </summary>
        /// <description>Updates an existing order.</description>
        /// <param name="id">The requested order identifier.</param>
        /// <param name="order">The order to update.</param>
        /// <returns>None.</returns>
        /// <response code="204">The order was successfully updated.</response>
        /// <response code="400">The order is invalid.</response>
        /// <response code="404">The order does not exist.</response>
        public static IResult Patch( int id, Models.V1.Order order ) => Results.NoContent();
    }

    /// <summary>
    /// Represents the 2.0 Orders service.
    /// </summary>
    public static class V2
    {
        /// <summary>
        /// Get Orders
        /// </summary>
        /// <description>Retrieves all orders.</description>
        /// <returns>All available orders.</returns>
        /// <response code="200">The successfully retrieved orders.</response>
        public static Models.V2.Order[] GetAll() =>
        [
            new (){ Id = 1, Customer = "John Doe" },
            new (){ Id = 2, Customer = "Bob Smith" },
            new (){ Id = 3, Customer = "Jane Doe", EffectiveDate = DateTimeOffset.UtcNow.AddDays( 7d ) },
        ];

        /// <summary>
        /// Get Order
        /// </summary>
        /// <description>Gets a single order.</description>
        /// <param name="id">The requested order identifier.</param>
        /// <returns>The requested order.</returns>
        /// <response code="200">The order was successfully retrieved.</response>
        /// <response code="404">The order does not exist.</response>
        public static Models.V2.Order GetById( int id ) => new() { Id = id, Customer = "John Doe" };

        /// <summary>
        /// Place Order
        /// </summary>
        /// <description>Places a new order.</description>
        /// <param name="request"></param>
        /// <param name="order">The order to place.</param>
        /// <returns>The created order.</returns>
        /// <response code="201">The order was successfully placed.</response>
        /// <response code="400">The order is invalid.</response>
        public static IResult Post( HttpRequest request, Models.V2.Order order )
        {
            order.Id = 42;
            var scheme = request.Scheme;
            var host = request.Host;
            var location = new Uri( $"{scheme}{Uri.SchemeDelimiter}{host}/api/orders/{order.Id}" );
            return Results.Created( location, order );
        }

        /// <summary>
        /// Update Order
        /// </summary>
        /// <description>Updates an existing order.</description>
        /// <param name="id">The requested order identifier.</param>
        /// <param name="order">The order to update.</param>
        /// <returns>None.</returns>
        /// <response code="204">The order was successfully updated.</response>
        /// <response code="400">The order is invalid.</response>
        /// <response code="404">The order does not exist.</response>
        public static IResult Patch( int id, Models.V2.Order order ) => Results.NoContent();
    }

    /// <summary>
    /// Represents the 3.0 Orders service.
    /// </summary>
    public static class V3
    {
        /// <summary>
        /// Get Orders
        /// </summary>
        /// <description>Retrieves all orders.</description>
        /// <returns>All available orders.</returns>
        /// <response code="200">The successfully retrieved orders.</response>
        public static Models.V3.Order[] GetAll() =>
        [
            new (){ Id = 1, Customer = "John Doe" },
            new (){ Id = 2, Customer = "Bob Smith" },
            new (){ Id = 3, Customer = "Jane Doe", EffectiveDate = DateTimeOffset.UtcNow.AddDays( 7d ) },
        ];

        /// <summary>
        /// Get Order
        /// </summary>
        /// <description>Gets a single order.</description>
        /// <param name="id">The requested order identifier.</param>
        /// <returns>The requested order.</returns>
        /// <response code="200">The order was successfully retrieved.</response>
        /// <response code="404">The order does not exist.</response>
        public static Models.V3.Order GetById( int id ) => new() { Id = id, Customer = "John Doe" };

        /// <summary>
        /// Place Order
        /// </summary>
        /// <description>Places a new order.</description>
        /// <param name="request"></param>
        /// <param name="order">The order to place.</param>
        /// <returns>The created order.</returns>
        /// <response code="201">The order was successfully placed.</response>
        /// <response code="400">The order is invalid.</response>
        public static IResult Post( HttpRequest request, Models.V3.Order order )
        {
            order.Id = 42;
            var scheme = request.Scheme;
            var host = request.Host;
            var location = new Uri( $"{scheme}{Uri.SchemeDelimiter}{host}/api/orders/{order.Id}" );
            return Results.Created( location, order );
        }

        /// <summary>
        /// Cancel Order
        /// </summary>
        /// <description>Cancels an order.</description>
        /// <param name="id">The order to cancel.</param>
        /// <returns>None</returns>
        /// <response code="204">The order was successfully canceled.</response>
        /// <response code="404">The order does not exist.</response>
        public static IResult Delete( int id ) => Results.NoContent();
    }
}