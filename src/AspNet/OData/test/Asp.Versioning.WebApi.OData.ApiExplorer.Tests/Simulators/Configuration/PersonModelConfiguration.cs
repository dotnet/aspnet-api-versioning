// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Simulators.Configuration;

using Asp.Versioning.OData;
using Asp.Versioning.Simulators.Models;
using Microsoft.AspNet.OData.Builder;

public class PersonModelConfiguration : IModelConfiguration
{
    public void Apply( ODataModelBuilder builder, ApiVersion apiVersion, string routePrefix ) =>
        builder.EntitySet<Person>( "People" );
}