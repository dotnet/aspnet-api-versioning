namespace System.Web.Http
{
    using Microsoft.Web.Http.Versioning;
    using System.Net.Http;

    /// <summary>
    /// Provides extension methods for the <see cref="HttpRequestMessage"/> class.
    /// </summary>
    public static class HttpRequestMessageExtensions
    {
        const string ODataApiVersionPropertiesKey = "MS_" + nameof( ODataApiVersionRequestProperties );

        /// <summary>
        /// Gets the current OData API versioning request properties.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage">request</see> to get the OData API versioning properties for.</param>
        /// <returns>The current <see cref="ODataApiVersionRequestProperties">OData API versioning properties</see>.</returns>
        public static ODataApiVersionRequestProperties ODataApiVersionProperties( this HttpRequestMessage request )
        {
            if ( request == null )
            {
                throw new ArgumentNullException( nameof( request ) );
            }

            if ( !request.Properties.TryGetValue( ODataApiVersionPropertiesKey, out var value ) || !( value is ODataApiVersionRequestProperties properties ) )
            {
                request.Properties[ODataApiVersionPropertiesKey] = properties = new ODataApiVersionRequestProperties();
            }

            return properties;
        }
    }
}