namespace Microsoft.AspNetCore.Mvc
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using System;

    /// <summary>
    /// Provides extension methods for the <see cref="HttpContext"/> class.
    /// </summary>
    [CLSCompliant( false )]
    public static class HttpContextExtensions
    {
        /// <summary>
        /// Gets the OData versioning feature.
        /// </summary>
        /// <param name="httpContext">The current <see cref="HttpContext">HTTP context</see>.</param>
        /// <returns>The <see cref="IODataVersioningFeature"/> associated with the current HTTP context.</returns>
        public static IODataVersioningFeature ODataVersioningFeature( this HttpContext httpContext )
        {
            if ( httpContext == null )
            {
                throw new ArgumentNullException( nameof( httpContext ) );
            }

            var features = httpContext.Features;
            var feature = features.Get<IODataVersioningFeature>();

            if ( feature == null )
            {
                features.Set( feature = new ODataVersioningFeature() );
            }

            return feature;
        }
    }
}