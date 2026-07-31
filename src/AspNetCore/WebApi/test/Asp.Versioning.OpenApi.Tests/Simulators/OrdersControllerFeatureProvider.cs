// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OpenApi.Simulators;

using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using System.Reflection;

/// <summary>
/// Registers <see cref="OrdersController"/>, which the default feature provider does not discover because it is
/// declared internal.
/// </summary>
internal sealed class OrdersControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
{
    public void PopulateFeature( IEnumerable<ApplicationPart> parts, ControllerFeature feature ) =>
        feature.Controllers.Add( typeof( OrdersController ).GetTypeInfo() );
}