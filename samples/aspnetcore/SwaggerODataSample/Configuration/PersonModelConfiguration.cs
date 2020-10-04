namespace Microsoft.Examples.Configuration
{
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Examples.Models;
    using System;

    /// <summary>
    /// Represents the model configuration for people.
    /// </summary>
    public class PersonModelConfiguration : IModelConfiguration
    {
        /// <inheritdoc />
        public void Apply( ODataModelBuilder builder, ApiVersion apiVersion, string routePrefix )
        {
            var person = builder.EntitySet<Person>( "People" ).EntityType;
            var address = builder.EntityType<Address>().HasKey( a => a.Id );

            person.HasKey( p => p.Id );
            person.Select().OrderBy( "firstName", "lastName" );

            if ( apiVersion < ApiVersions.V3 )
            {
                person.Ignore( p => p.Phone );
            }

            if ( apiVersion <= ApiVersions.V1 )
            {
                person.Ignore( p => p.Email );
            }

            if ( apiVersion > ApiVersions.V1 )
            {
                person.ContainsOptional( p => p.HomeAddress );
                person.Ignore( p => p.WorkAddress );

                var function = person.Collection.Function( "NewHires" );

                function.Parameter<DateTime>( "Since" );
                function.ReturnsFromEntitySet<Person>( "People" );
            }

            if ( apiVersion > ApiVersions.V2 )
            {
                person.ContainsOptional( p => p.WorkAddress );
                person.Action( "Promote" ).Parameter<string>( "title" );
            }
        }
    }
}