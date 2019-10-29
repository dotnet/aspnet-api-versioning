namespace Microsoft.AspNet.OData.Builder
{
    using System;

    /// <summary>
    /// Represents the settings for OData query options.
    /// </summary>
    public partial class ODataQueryOptionSettings
    {
        IODataQueryOptionDescriptionProvider? descriptionProvider;

        /// <summary>
        /// Gets or sets a value indicating whether query options have the system "$" prefix.
        /// </summary>
        /// <value>True if the OData query options use the "$" prefix; otherwise, false. The default
        /// value is <c>false</c>.</value>
        public bool NoDollarPrefix { get; set; }

        /// <summary>
        /// Gets or sets the provider used to describe query options.
        /// </summary>
        /// <value>The <see cref="IODataQueryOptionDescriptionProvider">provider</see> used to describe OData query options.</value>
        public IODataQueryOptionDescriptionProvider DescriptionProvider
        {
            get => descriptionProvider ??= new DefaultODataQueryOptionDescriptionProvider();
            set => descriptionProvider = value;
        }
    }
}