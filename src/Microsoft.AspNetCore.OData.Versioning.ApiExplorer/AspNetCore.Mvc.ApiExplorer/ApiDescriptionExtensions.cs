namespace Microsoft.AspNetCore.Mvc.ApiExplorer
{
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.OData.Edm;
    using System;

    /// <summary>
    /// Provides extension methods for the <see cref="ApiDescription"/> class.
    /// </summary>
    [CLSCompliant( false )]
    public static class ApiDescriptionExtensions
    {
        /// <summary>
        /// Gets the entity data model (EDM) associated with the API description.
        /// </summary>
        /// <param name="apiDescription">The <see cref="ApiDescription">API description</see> to get the model for.</param>
        /// <returns>The associated <see cref="IEdmModel">EDM model</see> or <c>null</c> if there is no associated model.</returns>
        public static IEdmModel? EdmModel( this ApiDescription apiDescription ) => apiDescription.GetProperty<IEdmModel>();

        /// <summary>
        /// Gets the entity set associated with the API description.
        /// </summary>
        /// <param name="apiDescription">The <see cref="ApiDescription">API description</see> to get the entity set for.</param>
        /// <returns>The associated <see cref="IEdmEntitySet">entity set</see> or <c>null</c> if there is no associated entity set.</returns>
        public static IEdmEntitySet? EntitySet( this ApiDescription apiDescription )
        {
            if ( apiDescription == null )
            {
                throw new ArgumentNullException( nameof( apiDescription ) );
            }

            var key = typeof( IEdmEntitySet );

            if ( apiDescription.Properties.TryGetValue( key, out var value ) )
            {
                return (IEdmEntitySet) value;
            }

            var container = apiDescription.EdmModel()?.EntityContainer;

            if ( container == null || !( apiDescription.ActionDescriptor is ControllerActionDescriptor descriptor ) )
            {
                return default;
            }

            var entitySetName = descriptor.ControllerName;
            var entitySet = container.FindEntitySet( entitySetName );

            descriptor.Properties[key] = entitySet;

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
        public static IEdmOperation? Operation( this ApiDescription apiDescription ) => apiDescription.GetProperty<IEdmOperation>();
    }
}