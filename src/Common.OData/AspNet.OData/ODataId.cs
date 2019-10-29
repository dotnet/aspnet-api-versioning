namespace Microsoft.AspNet.OData
{
    using Newtonsoft.Json;
    using System;

    /// <summary>
    /// Represents an OData identifier specified in the body of a POST or PUT OData relationship reference request.
    /// </summary>
    public class ODataId
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The <see cref="Uri">URL</see> representing the related entity identifier.</value>
        [JsonProperty( "@odata.id" )]
        public Uri Value { get; set; } = default!;
    }
}