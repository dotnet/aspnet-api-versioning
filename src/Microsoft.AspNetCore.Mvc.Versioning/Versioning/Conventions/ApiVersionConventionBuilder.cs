namespace Microsoft.AspNetCore.Mvc.Versioning.Conventions
{
    using ApplicationModels;
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
            Arg.NotNull( controllerModel, nameof( controllerModel ) );
            return InternalApplyTo( controllerModel );
        }

        static TypeInfo GetKey( Type type ) => type.GetTypeInfo();
    }
}