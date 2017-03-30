﻿namespace System.Web.Http
{
    using Microsoft;
    using Microsoft.Web.Http.Description;
    using System.Diagnostics.Contracts;
    using System.Web.Http.Description;

    /// <summary>
    /// Provides extension methods for the <see cref="HttpConfiguration"/> class.
    /// </summary>
    public static class HttpConfigurationExtensions
    {
        /// <summary>
        /// Adds or replaces the configured <see cref="IApiExplorer">API explorer</see> with an implementation that supports API versioning.
        /// </summary>
        /// <param name="configuration">The <see cref="HttpConfiguration">configuration</see> used to add the API explorer.</param>
        /// <returns>The newly registered <see cref="VersionedApiExplorer">versioned API explorer</see>.</returns>
        /// <remarks>This method always replaces the <see cref="IApiExplorer"/> with a new instance of <see cref="VersionedApiExplorer"/>.</remarks>
        public static VersionedApiExplorer AddVersionedApiExplorer( this HttpConfiguration configuration )
        {
            Arg.NotNull( configuration, nameof( configuration ) );
            Contract.Ensures( Contract.Result<VersionedApiExplorer>() != null );

            var apiExplorer = new VersionedApiExplorer( configuration );
            configuration.Services.Replace( typeof( IApiExplorer ), apiExplorer );
            return apiExplorer;
        }
    }
}