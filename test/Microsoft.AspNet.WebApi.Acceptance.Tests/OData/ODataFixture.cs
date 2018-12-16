namespace Microsoft.AspNet.OData
{
    using Microsoft.AspNet.OData.Routing;
    using Microsoft.OData.UriParser;
    using Microsoft.Web;
    using static Microsoft.OData.ODataUrlKeyDelimiter;

    public abstract class ODataFixture : HttpServerFixture
    {
        protected ODataFixture() => FilteredControllerTypes.Add( typeof( VersionedMetadataController ) );

        public IODataPathHandler PathHandler { get; } = new DefaultODataPathHandler() { UrlKeyDelimiter = Parentheses };

        public ODataUriResolver UriResolver { get; } = new UnqualifiedCallAndEnumPrefixFreeResolver() { EnableCaseInsensitive = true };
    }
}