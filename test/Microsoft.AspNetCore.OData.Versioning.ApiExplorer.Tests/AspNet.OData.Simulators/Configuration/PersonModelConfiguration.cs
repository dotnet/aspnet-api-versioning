namespace Microsoft.AspNet.OData.Simulators.Configuration
{
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.AspNet.OData.Simulators.Models;
    using Microsoft.AspNetCore.Mvc;
    using System;

    /// <summary>
    /// Represents the model configuration for people.
    /// </summary>
    public class PersonModelConfiguration : IModelConfiguration
    {
        /// <summary>
        /// Applies model configurations using the provided builder for the specified API version.
        /// </summary>
        /// <param name="builder">The <see cref="ODataModelBuilder">builder</see> used to apply configurations.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> associated with the <paramref name="builder"/>.</param>
        public void Apply( ODataModelBuilder builder, ApiVersion apiVersion )
        {
            var person = builder.EntitySet<Person>( "People" ).EntityType.HasKey( p => p.Id );

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
                var function = person.Collection.Function( "NewHires" );

                function.Parameter<DateTime>( "Since" );
                function.ReturnsFromEntitySet<Person>( "People" );
            }

            if ( apiVersion > ApiVersions.V2 )
            {
                person.Action( "Promote" ).Parameter<string>( "title" );
            }
        }
    }
}