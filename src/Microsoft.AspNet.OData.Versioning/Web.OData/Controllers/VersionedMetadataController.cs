namespace Microsoft.Web.OData.Controllers
{
    using Http;
    using System;
    using System.Web.Http;
    using System.Web.OData;

    /// <summary>
    /// Represents a <see cref="ApiController">controller</see> for generating versioned OData service and metadata documents.
    /// </summary>
    /// <remarks>This controller is, itself, <see cref="ApiVersionNeutralAttribute">API version-neutral</see>.</remarks>
    [ApiVersionNeutral]
    public class VersionedMetadataController : MetadataController
    {
    }
}
