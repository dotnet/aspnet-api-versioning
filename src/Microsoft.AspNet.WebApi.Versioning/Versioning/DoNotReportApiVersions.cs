namespace Microsoft.Web.Http.Versioning
{
    using System;

    partial class DoNotReportApiVersions
    {
        DoNotReportApiVersions() { }

        internal static IReportApiVersions Instance { get; } = new DoNotReportApiVersions();
    }
}