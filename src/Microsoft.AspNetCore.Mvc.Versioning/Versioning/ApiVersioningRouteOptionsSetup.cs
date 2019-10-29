namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Options;
    using System;

    /// <summary>
    /// Represents the API versioning configuration for ASP.NET Core <see cref="RouteOptions">routing options</see>.
    /// </summary>
    [CLSCompliant( false )]
    public class ApiVersioningRouteOptionsSetup : IPostConfigureOptions<RouteOptions>
    {
        readonly IOptions<ApiVersioningOptions> versioningOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersioningRouteOptionsSetup"/> class.
        /// </summary>
        /// <param name="options">The <see cref="ApiVersioningOptions">API versioning options</see> used to configure the MVC options.</param>
        public ApiVersioningRouteOptionsSetup( IOptions<ApiVersioningOptions> options ) => versioningOptions = options;

        /// <inheritdoc />
        public virtual void PostConfigure( string name, RouteOptions options )
        {
            if ( options == null )
            {
                throw new ArgumentNullException( nameof( options ) );
            }

            var key = versioningOptions.Value.RouteConstraintName;
            options.ConstraintMap.Add( key, typeof( ApiVersionRouteConstraint ) );
        }
    }
}