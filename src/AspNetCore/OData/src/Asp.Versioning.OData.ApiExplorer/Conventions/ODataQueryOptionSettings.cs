// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

using Microsoft.AspNetCore.Mvc.ModelBinding;

/// <content>
/// Provides additional implementation specific to Microsoft ASP.NET Core.
/// </content>
[CLSCompliant( false )]
public partial class ODataQueryOptionSettings
{
    /// <summary>
    /// Gets or sets the configured model metadata provider.
    /// </summary>
    /// <value>The configured <see cref="IModelMetadataProvider">model metadata provider</see>.</value>
    public IModelMetadataProvider? ModelMetadataProvider { get; set; }
}