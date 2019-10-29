namespace Microsoft.AspNetCore.Mvc.Versioning.Conventions
{
    using Microsoft.AspNetCore.Mvc.ApplicationModels;
    using System;
    using System.Linq;
    using System.Reflection;

    /// <content>
    /// Provides additional implementation specific ASP.NET Core.
    /// </content>
    [CLSCompliant( false )]
    public partial class ApiVersionConventionBuilder
    {
        /// <summary>
        /// Applies the defined API version conventions to the specified controller.
        /// </summary>
        /// <param name="controllerModel">The <see cref="ControllerModel">controller model</see>
        /// to apply configured conventions to.</param>
        /// <returns>True if any conventions were applied to the
        /// <paramref name="controllerModel">controller model</paramref>; otherwise, false.</returns>
        public virtual bool ApplyTo( ControllerModel controllerModel )
        {
            if ( controllerModel == null )
            {
                throw new ArgumentNullException( nameof( controllerModel ) );
            }

            return InternalApplyTo( controllerModel );
        }

        static TypeInfo GetKey( Type type ) => type.GetTypeInfo();

        static bool HasDecoratedActions( ControllerModel controllerModel )
        {
            foreach ( var action in controllerModel.Actions )
            {
                var attributes = action.Attributes;

                if ( attributes.OfType<IApiVersionNeutral>().Any() )
                {
                    return true;
                }

                if ( attributes.OfType<IApiVersionProvider>().Any() )
                {
                    return true;
                }
            }

            return false;
        }
    }
}