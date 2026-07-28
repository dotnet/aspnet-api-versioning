// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Grpc.Tests;

using global::Grpc.Core;
using Google.Protobuf.WellKnownTypes;

// the API Explorer only reflects over the service and its descriptors; the implementations are never invoked
public sealed class TestOrdersService : Orders.OrdersBase
{
    public override Task<OrderReply> GetOrder( OrderIdRequest request, ServerCallContext context ) =>
        Task.FromResult( new OrderReply() );

    public override Task<OrderReply> PlaceOrder( OrderRequest request, ServerCallContext context ) =>
        Task.FromResult( new OrderReply() );

    public override Task<Empty> ReplaceOrder( OrderRequest request, ServerCallContext context ) =>
        Task.FromResult( new Empty() );

    public override Task<OrderReply> GetOrderByVersion( OrderIdRequest request, ServerCallContext context ) =>
        Task.FromResult( new OrderReply() );
}