namespace Microsoft.Examples.Configuration
{
    using Microsoft.Web.Http;
    using Microsoft.Web.OData.Builder;
    using System.Web.OData.Builder;

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
            if ( apiVersion <= ApiVersions.V1 )
            {
                builder.EntitySet<V1.Models.Person>( "People" ).EntityType.HasKey( p => p.Id );
            }
            else if ( apiVersion == ApiVersions.V2 )
            {
                builder.EntitySet<V2.Models.Person>( "People" ).EntityType.HasKey( p => p.Id );
            }
            else if ( apiVersion == ApiVersions.V3 )
            {
                builder.EntitySet<V3.Models.Person>( "People" ).EntityType.HasKey( p => p.Id );
            }
        }
    }
}