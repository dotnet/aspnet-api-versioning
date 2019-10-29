namespace Microsoft.AspNet.OData.Builder
{
    using System;
    using System.Reflection;
#if WEBAPI
    using System.Web.Http.Controllers;
#endif

    /// <summary>
    /// Defines the behavior of an OData query options convention builder for an action.
    /// </summary>
    /// <typeparam name="T">The type of item the convention builder is for.</typeparam>
#if !WEBAPI
    [CLSCompliant( false )]
#endif
    public interface IODataActionQueryOptionsConventionBuilder<T>
        where T : notnull
#if WEBAPI
#pragma warning disable SA1001 // Commas should be spaced correctly
       , IHttpController
#pragma warning restore SA1001 // Commas should be spaced correctly
#endif
    {
        /// <summary>
        /// Gets or creates a convention builder for the specified controller action method.
        /// </summary>
        /// <param name="actionMethod">The controller action <see cref="MethodInfo">method</see>
        /// to get or create a convention for.</param>
        /// <returns>A new or existing <see cref="ODataActionQueryOptionsConventionBuilder{T}"/>.</returns>
        ODataActionQueryOptionsConventionBuilder<T> Action( MethodInfo actionMethod );
    }
}