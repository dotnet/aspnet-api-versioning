// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable CA1062 // Validate arguments of public methods

namespace Asp.Versioning.Simulators.Configuration;

using Asp.Versioning.OData;
using Asp.Versioning.Simulators.Models;
using Microsoft.AspNet.OData.Builder;

public class OrderModelConfiguration : IModelConfiguration
{
    public void Apply( ODataModelBuilder builder, ApiVersion apiVersion, string routePrefix ) =>
        builder.EntitySet<Order>( "Orders" );
}