namespace Microsoft.Examples.Configuration
{
    using Microsoft.Web.Http;
    using Microsoft.Web.OData.Builder;
    using Models;
    using System.Web.OData.Builder;

    /// <summary>
    /// Represents the model configuration for <see cref="Person">people</see>.
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
            var person = builder.EntitySet<Person>( "People" ).EntityType;

            person.HasKey( p => p.Id );

            if ( apiVersion < ApiVersions.V3 )
            {
                person.Ignore( p => p.Phone );
            }

            if ( apiVersion < ApiVersions.V2 )
            {
                person.Ignore( p => p.Email );
            }
        }
    }
}