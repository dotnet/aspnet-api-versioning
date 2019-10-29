#if WEBAPI
namespace Microsoft.Web.Http.Versioning.Conventions
#else
namespace Microsoft.AspNetCore.Mvc.Versioning.Conventions
#endif
{
    using System;

    /// <summary>
    /// Defines the behavior of an API version convention.
    /// </summary>
    /// <typeparam name="T">The <see cref="Type">type</see> of item to apply the convention to.</typeparam>
    public interface IApiVersionConvention<in T> where T : notnull
    {
        /// <summary>
        /// Applies the API version convention.
        /// </summary>
        /// <param name="item">The descriptor to apply the convention to.</param>
        void ApplyTo( T item );
    }
}