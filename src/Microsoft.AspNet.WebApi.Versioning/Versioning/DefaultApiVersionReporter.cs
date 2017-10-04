namespace Microsoft.Web.Http.Versioning
{
    using System;

    partial class DefaultApiVersionReporter
    {
        DefaultApiVersionReporter() { }

        internal static IReportApiVersions Instance { get; } = new DefaultApiVersionReporter();
    }
}