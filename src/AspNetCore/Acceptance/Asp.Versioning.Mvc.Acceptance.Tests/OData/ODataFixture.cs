// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData;

using Asp.Versioning.Controllers;
using Asp.Versioning.OData.Configuration;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.OData;
using Microsoft.Extensions.DependencyInjection;

public abstract class ODataFixture : HttpServerFixture
{
    protected ODataFixture() => FilteredControllerTypes.Add( typeof( VersionedMetadataController ) );

    protected override void OnConfigurePartManager( ApplicationPartManager partManager )
    {
        base.OnConfigurePartManager( partManager );

        partManager.ApplicationParts.Add(
            new TestApplicationPart(
                typeof( OrderModelConfiguration ),
                typeof( PersonModelConfiguration ),
                typeof( CustomerModelConfiguration ),
                typeof( WeatherForecastModelConfiguration ) ) );
    }

    protected override void OnConfigureServices( IServiceCollection services ) =>
        services.AddProblemDetails().AddControllers().AddOData();

    protected override void OnAddApiVersioning( IApiVersioningBuilder builder ) =>
        builder.AddOData( OnEnableOData );

    protected virtual void OnEnableOData( ODataApiVersioningOptions options ) { }
}