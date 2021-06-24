namespace Microsoft.AspNetCore.OData.Basic.Controllers.Classic
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.OData.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    [ApiVersion( "1.0" )]
    public class WeatherForecastsController : ODataController
    {
        readonly Random randomNumberGenerator = new();

        public IEnumerable<WeatherForecast> GetWeatherForecasts() =>
            Enumerable.Range( 1, 3 ).Select( index => new WeatherForecast
            {
                Id = Guid.NewGuid().ToString(),
                Date = DateTime.Now.AddDays( index ),
                Temperature = randomNumberGenerator.Next( -20, 55 ),
                Summary = "Forecast",
            } );

        public WeatherForecast GetWeatherForecast( string key ) =>
            new()
            {
                Id = key,
                Date = DateTime.Today,
                Temperature = randomNumberGenerator.Next( -20, 55 ),
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
                Temperature = randomNumberGenerator.Next( -20, 55 ),
                Summary = "Forecast",
            };
            delta.Patch( existing );
            return existing;
        }

        public void DeleteWeatherForecast( string key ) { }
    }
}