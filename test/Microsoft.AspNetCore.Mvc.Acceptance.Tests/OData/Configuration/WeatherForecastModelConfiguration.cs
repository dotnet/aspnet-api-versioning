namespace Microsoft.AspNetCore.OData.Configuration
{
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.OData.Models;

    public class WeatherForecastModelConfiguration : IModelConfiguration
    {
        readonly ApiVersion supportedApiVersion;

        public WeatherForecastModelConfiguration() { }

        public WeatherForecastModelConfiguration( ApiVersion supportedApiVersion ) => this.supportedApiVersion = supportedApiVersion;

        EntityTypeConfiguration<WeatherForecast> ConfigureCurrent( ODataModelBuilder builder )
        {
            var forecast = builder.EntitySet<WeatherForecast>( "WeatherForecasts" ).EntityType;
            forecast.HasKey( p => p.Id );
            return forecast;
        }

        public void Apply( ODataModelBuilder builder, ApiVersion apiVersion, string routePrefix )
        {
            if ( supportedApiVersion == null || supportedApiVersion == apiVersion )
            {
                ConfigureCurrent( builder );
            }
        }
    }
}