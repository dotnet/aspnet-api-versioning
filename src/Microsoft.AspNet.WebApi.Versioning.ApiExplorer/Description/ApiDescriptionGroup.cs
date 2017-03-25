namespace Microsoft.Web.Http.Description
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;

    /// <summary>
    /// Represents a group of versioned API descriptions.
    /// </summary>
    [DebuggerDisplay( "Version = {Version}, Count = {ApiDescriptions.Count}" )]
    public class ApiDescriptionGroup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApiDescriptionGroup"/> class.
        /// </summary>
        /// <param name="version">The <see cref="ApiVersion">version</see> associated with the group.</param>
        public ApiDescriptionGroup( ApiVersion version )
        {
            Arg.NotNull( version, nameof( version ) );
            Version = version;
        }

        /// <summary>
        /// Gets the version associated with the group of APIs.
        /// </summary>
        /// <value>An <see cref="ApiVersion">API version</see>.</value>
        public ApiVersion Version { get; }

        /// <summary>
        /// Gets a collection of API descriptions for the current version.
        /// </summary>
        /// <value>A <see cref="Collection{T}">collection</see> of
        /// <see cref="VersionedApiDescription">versioned API descriptions</see>.</value>
        public virtual Collection<VersionedApiDescription> ApiDescriptions { get; } = new Collection<VersionedApiDescription>();
    }
}