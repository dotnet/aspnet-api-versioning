namespace System.Web.Http.Description
{
    using Microsoft.OData.Edm;
    using Microsoft.Web.Http.Description;

    /// <summary>
    /// Provides extension methods for the <see cref="ApiDescription"/> class.
    /// </summary>
    public static class ApiDescriptionExtensions
    {
        /// <summary>
        /// Gets the entity data model (EDM) associated with the API description.
        /// </summary>
        /// <param name="apiDescription">The <see cref="ApiDescription">API description</see> to get the model for.</param>
        /// <returns>The associated <see cref="IEdmModel">EDM model</see> or <c>null</c> if there is no associated model.</returns>
        public static IEdmModel? EdmModel( this ApiDescription apiDescription )
        {
            if ( apiDescription is VersionedApiDescription description )
            {
                return description.GetProperty<IEdmModel>();
            }

            return default;
        }

        /// <summary>
        /// Gets the entity set associated with the API description.
        /// </summary>
        /// <param name="apiDescription">The <see cref="ApiDescription">API description</see> to get the entity set for.</param>
        /// <returns>The associated <see cref="IEdmEntitySet">entity set</see> or <c>null</c> if there is no associated entity set.</returns>
        public static IEdmEntitySet? EntitySet( this ApiDescription apiDescription )
        {
            if ( !( apiDescription is VersionedApiDescription description ) )
            {
                return default;
            }

            var key = typeof( IEdmEntitySet );

            if ( description.Properties.TryGetValue( key, out var value ) )
            {
                return (IEdmEntitySet) value;
            }

            var container = description.EdmModel()?.EntityContainer;

            if ( container == null )
            {
                return default;
            }

            var entitySetName = description.ActionDescriptor.ControllerDescriptor.ControllerName;
            var entitySet = container.FindEntitySet( entitySetName );

            description.Properties[key] = entitySet;

            return entitySet;
        }

        /// <summary>
        /// Gets the entity type associated with the API description.
        /// </summary>
        /// <param name="apiDescription">The <see cref="ApiDescription">API description</see> to get the entity type for.</param>
        /// <returns>The associated <see cref="IEdmEntityType">entity type</see> or <c>null</c> if there is no associated entity type.</returns>
        public static IEdmEntityType? EntityType( this ApiDescription apiDescription ) => apiDescription.EntitySet()?.EntityType();

        /// <summary>
        /// Gets the operation associated with the API description.
        /// </summary>
        /// <param name="apiDescription">The <see cref="ApiDescription">API description</see> to get the operation for.</param>
        /// <returns>The associated <see cref="IEdmOperation">EDM operation</see> or <c>null</c> if there is no associated operation.</returns>
        public static IEdmOperation? Operation( this ApiDescription apiDescription )
        {
            if ( apiDescription is VersionedApiDescription description )
            {
                return description.GetProperty<IEdmOperation>();
            }

            return default;
        }
    }
}