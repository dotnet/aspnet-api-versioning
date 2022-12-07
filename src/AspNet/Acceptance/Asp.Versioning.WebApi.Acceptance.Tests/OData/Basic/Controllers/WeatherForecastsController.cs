// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0060 // Remove unused parameter

namespace Asp.Versioning.OData.Basic.Controllers;

using Asp.Versioning.OData.Models;
using Microsoft.AspNet.OData;
using System.Security.Cryptography;
using System.Web.Http;

[ApiVersion( "1.0" )]
public class WeatherForecastsController : ODataController
{
    private readonly RandomNumberGenerator rng = RandomNumberGenerator.Create();

    private int RandomTemperature()
    {
        var bytes = new byte[4];
        rng.GetBytes( bytes );
        return BitConverter.ToInt32( bytes, 0 );
    }

    public IEnumerable<WeatherForecast> GetWeatherForecasts() =>
        Enumerable.Range( 1, 3 ).Select( index => new WeatherForecast
        {
            Id = Guid.NewGuid().ToString(),
            Date = DateTime.Now.AddDays( index ),
            Temperature = RandomTemperature(),
            Summary = "Forecast",
        } );

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