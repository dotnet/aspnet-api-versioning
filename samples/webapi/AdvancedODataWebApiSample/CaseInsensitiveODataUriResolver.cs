namespace Microsoft.Examples
{
    using Microsoft.OData.UriParser;
    using System;

    // HACK: required due to bug in ODL
    // REF: https://github.com/OData/odata.net/issues/695
    public sealed class CaseInsensitiveODataUriResolver : UnqualifiedODataUriResolver
    {
        public override bool EnableCaseInsensitive { get { return true; } set { } }
    }
}