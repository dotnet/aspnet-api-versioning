using ApiVersioning.Examples.Models;
using Asp.Versioning;
using Asp.Versioning.OData;
using Microsoft.OData.ModelBuilder;

namespace ApiVersioning.Examples.Configuration
{
    public class PersonModelConfiguration : IModelConfiguration
    {
        public void Apply( ODataModelBuilder builder, ApiVersion apiVersion, string routePrefix )
        {
            builder.EntitySet<Person>( "People" );
        }
    }
}
