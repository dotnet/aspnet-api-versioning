namespace Microsoft.AspNetCore.OData.Basic.Controllers.Endpoint
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

        [HttpGet]
        public IEnumerable<WeatherForecast> GetWeatherForecasts() =>
            Enumerable.Range( 1, 3 ).Select( index => new WeatherForecast
            {
                Id = Guid.NewGuid().ToString(),
                Date = DateTime.Now.AddDays( index ),
                Temperature = randomNumberGenerator.Next( -20, 55 ),
                Summary = "Forecast",
            } );

        [HttpGet( "{id}" )]
        public WeatherForecast GetWeatherForecast( string id ) =>
            new()
            {
                Id = id,
                Date = DateTime.Today,
                Temperature = randomNumberGenerator.Next( -20, 55 ),
                Summary = "Forecast",
            };

        [HttpPost]
        public WeatherForecast PostWeatherForecast( [FromBody] WeatherForecast forecast )
        {
            forecast.Id = Guid.NewGuid().ToString();
            return forecast;
        }

        [HttpPut( "{id}" )]
        public WeatherForecast PutWeatherForecast( string id, [FromBody] WeatherForecast forecast )
        {
            forecast.Id = id;
            return forecast;
        }

        [HttpPatch( "{id}" )]
        public WeatherForecast PatchWeatherForecast( string id, Delta<WeatherForecast> delta )
        {
            var existing = new WeatherForecast()
            {
                Id = id,
                Date = DateTime.Today,
                Temperature = randomNumberGenerator.Next( -20, 55 ),
                Summary = "Forecast",
            };
            delta.Patch( existing );
            return existing;
        }

        [HttpDelete( "{id}" )]
        public void DeleteWeatherForecast( string id ) { }
    }
}