namespace Microsoft.Web.Http.Versioning.Conventions
{
    using System;
    using System.Reflection;
    using System.Web.Http.Controllers;

    /// <content>
    /// Provides additional implementation specific to Microsoft ASP.NET Web API.
    /// </content>
    public partial class ControllerApiVersionConventionBuilder
    {
        /// <summary>
        /// Gets a value indicating whether the builder has any related action conventions.
        /// </summary>
        /// <value>True if the builder has related action conventions; otherwise, false.</value>
        protected override bool HasActionConventions => ActionBuilders.Count > 0;

        /// <summary>
        /// Attempts to get the convention for the specified action method.
        /// </summary>
        /// <param name="method">The <see cref="MethodInfo">method</see> representing the action to retrieve the convention for.</param>
        /// <param name="convention">The retrieved <see cref="IApiVersionConvention{T}">convention</see> or <c>null</c>.</param>
        /// <returns>True if the convention was successfully retrieved; otherwise, false.</returns>
        protected override bool TryGetConvention( MethodInfo method, out IApiVersionConvention<HttpActionDescriptor> convention )
        {
            Arg.NotNull( method, nameof( method ) );

            if ( ActionBuilders.TryGetValue( method, out var builder ) )
            {
                convention = builder;
                return true;
            }

            convention = default;
            return false;
        }
    }
}