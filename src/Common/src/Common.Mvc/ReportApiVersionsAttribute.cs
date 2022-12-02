// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

#if NETFRAMEWORK
using System.Web.Http.Filters;
#else
using Microsoft.AspNetCore.Mvc.Filters;
#endif
using static System.AttributeTargets;

/// <summary>
/// Represents an <see cref="ActionFilterAttribute">action filter</see> which reports API version information for
/// an entire service or specific service action.
/// </summary>
[AttributeUsage( Class | Method, Inherited = true, AllowMultiple = false )]
public sealed partial class ReportApiVersionsAttribute : ActionFilterAttribute
{
    private readonly IReportApiVersions? reportApiVersions;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportApiVersionsAttribute"/> class.
    /// </summary>
    public ReportApiVersionsAttribute() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportApiVersionsAttribute"/> class.
    /// </summary>
    /// <param name="reportApiVersions">The <see cref="IReportApiVersions">object</see> used to report API versions.</param>
    public ReportApiVersionsAttribute( IReportApiVersions reportApiVersions ) => this.reportApiVersions = reportApiVersions;
}