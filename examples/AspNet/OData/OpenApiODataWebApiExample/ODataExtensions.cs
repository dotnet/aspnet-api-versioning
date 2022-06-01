namespace ApiVersioning.Examples;

using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData.UriParser;
using System.Web.Http;

internal static class ODataExtensions
{
    public static IReadOnlyDictionary<string, object> GetRelatedKeys( this ApiController controller, Uri uri )
    {
        var request = controller.Request;
        var pathHandler = request.GetPathHandler();
        var serviceRoot = controller.Url.CreateODataLink();
        var path = pathHandler.Parse( serviceRoot, uri.AbsoluteUri, request.GetRequestContainer() );
        var keys = new Dictionary<string, object>( StringComparer.OrdinalIgnoreCase );

        if ( path.Segments.OfType<KeySegment>().FirstOrDefault<KeySegment>() is KeySegment segment )
        {
            foreach ( var pair in segment.Keys )
            {
                keys.Add( pair.Key, pair.Value );
            }
        }

        return keys;
    }

    public static object GetRelatedKey( this ApiController controller, Uri uri ) => controller.GetRelatedKeys( uri ).Values.SingleOrDefault();
}