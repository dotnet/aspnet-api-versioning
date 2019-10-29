namespace Microsoft.Web.Http.Description
{
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    /// Represents a group of versioned API descriptions.
    /// </summary>
    [DebuggerDisplay( "ApiVersion = {ApiVersion}, Count = {ApiDescriptions.Count}" )]
    public class ApiDescriptionGroup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApiDescriptionGroup"/> class.
        /// </summary>
        /// <param name="apiVersion">The <see cref="Http.ApiVersion">API version</see> associated with the group.</param>
        public ApiDescriptionGroup( ApiVersion apiVersion ) => ApiVersion = apiVersion;

        /// <summary>
        /// Gets the version associated with the group of APIs.
        /// </summary>
        /// <value>An <see cref="Http.ApiVersion">API version</see>.</value>
        public ApiVersion ApiVersion { get; }

        /// <summary>
        /// Gets or sets the name of the API description group.
        /// </summary>
        /// <value>The API version description group name.</value>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets a value indicating whether API version is deprecated for all described APIs in the group.
        /// </summary>
        /// <value>True if all APIs in the group are deprecated; otherwise, false.</value>
        /// <remarks>An API version will only be described as deprecated when all
        /// all corresponding service implementations are also deprecated. It is
        /// possible that some API versions may be partially deprecated, in which
        /// case this property will return <c>false</c>, but individual actions
        /// may report that they are deprecated.</remarks>
        public virtual bool IsDeprecated => ApiDescriptions.All( d => d.IsDeprecated );

        /// <summary>
        /// Gets a collection of API descriptions for the current version.
        /// </summary>
        /// <value>A <see cref="Collection{T}">collection</see> of
        /// <see cref="VersionedApiDescription">versioned API descriptions</see>.</value>
        public virtual Collection<VersionedApiDescription> ApiDescriptions { get; } = new Collection<VersionedApiDescription>();
    }
}