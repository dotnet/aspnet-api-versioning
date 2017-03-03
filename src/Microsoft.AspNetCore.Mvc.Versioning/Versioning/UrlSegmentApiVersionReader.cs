namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using AspNetCore.Routing;
    using Http;
    using System;
    using static System.String;

    /// <content>
    /// Provides the implementation for ASP.NET Core.
    /// </content>
    [CLSCompliant( false )]
    public partial class UrlSegmentApiVersionReader
    {
        /// <summary>
        /// Reads the service API version value from a request.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequest">HTTP request</see> to read the API version from.</param>
        /// <returns>The raw, unparsed service API version value read from the request or <c>null</c> if request does not contain an API version.</returns>
        /// <exception cref="AmbiguousApiVersionException">Multiple, different API versions were requested.</exception>
        public virtual string Read( HttpRequest request )
        {
            Arg.NotNull( request, nameof( request ) );

            var context = request.HttpContext;
            var key = context.GetRouteParameterNameAssignedByApiVersionRouteConstraint();

            if ( IsNullOrEmpty( key ) )
            {
                return null;
            }

            return context.GetRouteValue( key )?.ToString();
        }
    }
}