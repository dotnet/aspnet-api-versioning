// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

#if NETFRAMEWORK
using System.Web.Http.Description;
#else
using Microsoft.AspNetCore.Mvc.ApiExplorer;
#endif

/// <summary>
/// Defines the behavior of an OData query options convention.
/// </summary>
#if !NETFRAMEWORK
[CLSCompliant( false )]
#endif
public interface IODataQueryOptionsConvention
{
    /// <summary>
    /// Applies the convention to the specified API description.
    /// </summary>
    /// <param name="apiDescription">The <see cref="ApiDescription">API description</see> to apply the convention to.</param>
    void ApplyTo( ApiDescription apiDescription );
}