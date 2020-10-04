namespace Microsoft.AspNet.OData
{
    using Microsoft.Web;

    public abstract class ODataFixture : HttpServerFixture
    {
        protected ODataFixture() => FilteredControllerTypes.Add( typeof( VersionedMetadataController ) );
    }
}