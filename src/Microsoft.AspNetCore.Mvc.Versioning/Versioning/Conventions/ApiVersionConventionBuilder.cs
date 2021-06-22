namespace Microsoft.AspNetCore.Mvc.Versioning.Conventions
{
    using Microsoft.AspNetCore.Mvc.ApplicationModels;
    using System;
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
            for ( var i = 0; i < controllerModel.Actions.Count; i++ )
            {
                var action = controllerModel.Actions[i];

                for ( var j = 0; j < action.Attributes.Count; j++ )
                {
                    var attribute = action.Attributes[j];

                    if ( attribute is IApiVersionProvider || attribute is IApiVersionNeutral )
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}