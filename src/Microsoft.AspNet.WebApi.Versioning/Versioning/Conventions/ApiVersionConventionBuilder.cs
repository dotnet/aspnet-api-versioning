namespace Microsoft.Web.Http.Versioning.Conventions
{
    using System;
    using System.Web.Http.Controllers;

    /// <content>
    /// Provides additional implementation specific ASP.NET Web API.
    /// </content>
    public partial class ApiVersionConventionBuilder
    {
        /// <summary>
        /// Applies the defined API version conventions to the specified controller.
        /// </summary>
        /// <param name="controllerDescriptor">The <see cref="HttpControllerDescriptor">controller descriptor</see>
        /// to apply configured conventions to.</param>
        /// <returns>True if any conventions were applied to the
        /// <paramref name="controllerDescriptor">controller descriptor</paramref>; otherwise, false.</returns>
        public virtual bool ApplyTo( HttpControllerDescriptor controllerDescriptor )
        {
            Arg.NotNull( controllerDescriptor, nameof( controllerDescriptor ) );
            return InternalApplyTo( controllerDescriptor );
        }

        static Type GetKey( Type type ) => type;
    }
}