namespace Microsoft.Web.Http.Versioning
{
    using System;

    partial class DoNotReportApiVersions
    {
        static DoNotReportApiVersions? instance;

        DoNotReportApiVersions() { }

        internal static IReportApiVersions Instance => instance ??= new DoNotReportApiVersions();
    }
}