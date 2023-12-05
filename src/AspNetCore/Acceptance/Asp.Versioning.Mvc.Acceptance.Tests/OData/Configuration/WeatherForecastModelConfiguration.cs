// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData.Configuration;

using Asp.Versioning.OData.Models;
using Microsoft.OData.ModelBuilder;

public class WeatherForecastModelConfiguration : IModelConfiguration
{
    private readonly ApiVersion supportedApiVersion;

    public WeatherForecastModelConfiguration() { }

    public WeatherForecastModelConfiguration( ApiVersion supportedApiVersion ) => this.supportedApiVersion = supportedApiVersion;

    private static EntityTypeConfiguration<WeatherForecast> ConfigureCurrent( ODataModelBuilder builder )
    {
        var forecast = builder.EntitySet<WeatherForecast>( "WeatherForecasts" ).EntityType;
        forecast.HasKey( p => p.Id );
        return forecast;
    }

    public void Apply( ODataModelBuilder builder, ApiVersion apiVersion, string routePrefix )
    {
        ArgumentNullException.ThrowIfNull( builder );

        if ( supportedApiVersion == null || supportedApiVersion == apiVersion )
        {
            ConfigureCurrent( builder );
        }
    }
}