namespace Microsoft.AspNetCore.Mvc.ApiExplorer
{
    using System;

    /// <summary>
    /// Defines the behavior of a formatter for API version group names.
    /// </summary>
    public interface IApiVersionGroupNameFormatter
    {
        /// <summary>
        /// Returns the group name for the specified API version.
        /// </summary>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> to retrieve a group name for.</param>
        /// <returns>The group name for the specified <paramref name="apiVersion">API version</paramref>.</returns>
        string GetGroupName( ApiVersion apiVersion );
    }
}