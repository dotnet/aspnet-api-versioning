namespace Microsoft.AspNetCore.Mvc.ApiExplorer
{
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Defines the behavior of a provider that discovers and describes API version information within an application.
    /// </summary>
    [CLSCompliant( false )]
    public interface IApiVersionDescriptionProvider
    {
        /// <summary>
        /// Gets a read-only list of discovered API version descriptions.
        /// </summary>
        /// <value>A <see cref="IReadOnlyList{T}">read-only list</see> of <see cref="ApiVersionDescription">API version descriptions</see>.</value>
        IReadOnlyList<ApiVersionDescription> ApiVersionDescriptions { get; }

        /// <summary>
        /// Determines whether the specified action is deprecated for the provided API version.
        /// </summary>
        /// <param name="actionDescriptor">The <see cref="ActionDescriptor">action</see> to evaluate.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> to evaluate.</param>
        /// <returns>True if the specified <paramref name="actionDescriptor">action</paramref> is deprecated for the
        /// <paramref name="apiVersion">API version</paramref>; otherwise, false.</returns>
        bool IsDeprecated( ActionDescriptor actionDescriptor, ApiVersion apiVersion );
    }
}