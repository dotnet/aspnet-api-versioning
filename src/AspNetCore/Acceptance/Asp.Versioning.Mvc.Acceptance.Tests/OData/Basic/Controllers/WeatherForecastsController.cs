// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0060 // Remove unused parameter
#pragma warning disable IDE0079
#pragma warning disable CA1822 // Mark members as static

namespace Asp.Versioning.OData.Basic.Controllers;

using Asp.Versioning.OData.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using System.Security.Cryptography;

[ApiVersion( 1.0 )]
public class WeatherForecastsController : ODataController
{
    private readonly RandomNumberGenerator rng = RandomNumberGenerator.Create();

    private int RandomTemperature()
    {
        var bytes = new byte[4];
        rng.GetBytes( bytes );
        return BitConverter.ToInt32( bytes, 0 );
    }

    [EnableQuery]
    public IEnumerable<WeatherForecast> GetWeatherForecasts() =>
        Enumerable.Range( 1, 3 ).Select( index => new WeatherForecast
        {
            Id = Guid.NewGuid().ToString(),
            Date = DateTime.Now.AddDays( index ),
            Temperature = RandomTemperature(),
            Summary = "Forecast",
        } );

    [EnableQuery]
    public WeatherForecast GetWeatherForecast( string key ) =>
        new()
        {
            Id = key,
            Date = DateTime.Today,
            Temperature = RandomTemperature(),
            Summary = "Forecast",
        };

    public WeatherForecast PostWeatherForecast( [FromBody] WeatherForecast forecast )
    {
        forecast.Id = Guid.NewGuid().ToString();
        return forecast;
    }

    public WeatherForecast PutWeatherForecast( string key, [FromBody] WeatherForecast forecast )
    {
        forecast.Id = key;
        return forecast;
    }

    public WeatherForecast PatchWeatherForecast( string key, Delta<WeatherForecast> delta )
    {
        var existing = new WeatherForecast()
        {
            Id = key,
            Date = DateTime.Today,
            Temperature = RandomTemperature(),
            Summary = "Forecast",
        };
        delta.Patch( existing );
        return existing;
    }

    public void DeleteWeatherForecast( string key ) { }
}