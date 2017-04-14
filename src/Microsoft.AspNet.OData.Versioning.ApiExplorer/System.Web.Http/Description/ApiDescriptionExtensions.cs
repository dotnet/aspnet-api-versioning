namespace System.Web.Http.Description
{
    using Microsoft;
    using Microsoft.OData.Edm;
    using Microsoft.Web.Http.Description;
    using System;

    /// <summary>
    /// Provides extension methods for the <see cref="ApiDescription"/> class.
    /// </summary>
    public static class ApiDescriptionExtensions
    {
        /// <summary>
        /// Gets the entity data model (EDM) associated with the API description.
        /// </summary>
        /// <param name="apiDescription">The <see cref="VersionedApiDescription">API description</see> to get the model for.</param>
        /// <returns>The associated <see cref="IEdmModel">EDM model</see> or <c>null</c> if there is no associated model.</returns>
        public static IEdmModel EdmModel( this VersionedApiDescription apiDescription ) => apiDescription.GetProperty<IEdmModel>();

        /// <summary>
        /// Gets the entity set associated with the API description.
        /// </summary>
        /// <param name="apiDescription">The <see cref="VersionedApiDescription">API description</see> to get the entity set for.</param>
        /// <returns>The associated <see cref="IEdmEntitySet">entity set</see> or <c>null</c> if there is no associated entity set.</returns>
        public static IEdmEntitySet EntitySet( this VersionedApiDescription apiDescription )
        {
            Arg.NotNull( apiDescription, nameof( apiDescription ) );

            var key = typeof( IEdmEntitySet );

            if ( apiDescription.Properties.TryGetValue( key, out object value ) )
            {
                return (IEdmEntitySet) value;
            }

            var container = apiDescription.EdmModel()?.EntityContainer;

            if ( container == null )
            {
                return null;
            }

            var entitySetName = apiDescription.ActionDescriptor.ControllerDescriptor.ControllerName;
            var entitySet = container.FindEntitySet( entitySetName );

            apiDescription.Properties[key] = entitySet;

            return entitySet;
        }

        /// <summary>
        /// Gets the entity type associated with the API description.
        /// </summary>
        /// <param name="apiDescription">The <see cref="VersionedApiDescription">API description</see> to get the entity type for.</param>
        /// <returns>The associated <see cref="IEdmEntityType">entity type</see> or <c>null</c> if there is no associated entity type.</returns>
        public static IEdmEntityType EntityType( this VersionedApiDescription apiDescription )
        {
            Arg.NotNull( apiDescription, nameof( apiDescription ) );
            return apiDescription.EntitySet()?.EntityType();
        }
    }
}