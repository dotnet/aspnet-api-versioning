namespace Microsoft.Examples
{
    using Microsoft.OData.UriParser;
    using System;

    /// <summary>
    /// Represents a case-insensitive URI resolver.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///  <item>HACK: required due to bug in ODL</item>
    ///  <item>REF: https://github.com/OData/odata.net/issues/695</item>
    /// </list></remarks>
    public sealed class CaseInsensitiveODataUriResolver : UnqualifiedODataUriResolver
    {
        /// <summary>
        /// Gets or sets whether the URI resolver is case-sensitive.
        /// </summary>
        /// <value>True if the URI resolver is case-sensitive; otherwise, false.</value>
        /// <remarks>This property will always return <c>false</c>.</remarks>
        public override bool EnableCaseInsensitive { get { return true; } set { } }
    }
}