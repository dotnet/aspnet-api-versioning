namespace ApiVersioning.Examples;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.OData;
using Microsoft.OData.UriParser;

internal static class ODataExtensions
{
    public static IReadOnlyDictionary<string, object> GetRelatedKeys( this ControllerBase controller, Uri uri )
    {
        // REF: https://github.com/OData/AspNetCoreOData/blob/main/src/Microsoft.AspNetCore.OData/Routing/Parser/DefaultODataPathParser.cs
        var feature = controller.HttpContext.ODataFeature();
        var model = feature.Model;
        var serviceRoot = new Uri( new Uri( feature.BaseAddress ), feature.RoutePrefix );
        var requestProvider = feature.Services;
        var parser = new ODataUriParser( model, serviceRoot, uri, requestProvider );

        parser.Resolver ??= new UnqualifiedODataUriResolver() { EnableCaseInsensitive = true };
        parser.UrlKeyDelimiter = ODataUrlKeyDelimiter.Slash;

        var path = parser.ParsePath();
        var segment = path.OfType<KeySegment>().FirstOrDefault<KeySegment>();

        if ( segment is null )
        {
            return new Dictionary<string, object>( capacity: 0 );
        }

        return new Dictionary<string, object>( segment.Keys, StringComparer.OrdinalIgnoreCase );
    }

    public static object GetRelatedKey( this ControllerBase controller, Uri uri ) => controller.GetRelatedKeys( uri ).Values.SingleOrDefault();
}