namespace Microsoft.Web.Http.Versioning.Conventions
{
    using System;
    using System.Linq;
    using System.Web.Http;
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
            if ( controllerDescriptor == null )
            {
                throw new ArgumentNullException( nameof( controllerDescriptor ) );
            }

            return InternalApplyTo( controllerDescriptor );
        }

        static Type GetKey( Type type ) => type;

        static bool HasDecoratedActions( HttpControllerDescriptor controllerDescriptor )
        {
            var actionSelector = controllerDescriptor.Configuration.Services.GetActionSelector();
            var actions = actionSelector.GetActionMapping( controllerDescriptor ).SelectMany( g => g );

            foreach ( var action in actions )
            {
                if ( action.GetCustomAttributes<IApiVersionNeutral>( inherit: true ).Count > 0 )
                {
                    return true;
                }

                if ( action.GetCustomAttributes<IApiVersionProvider>( inherit: false ).Count > 0 )
                {
                    return true;
                }
            }

            return false;
        }
    }
}