// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.OData.Query;

/// <content>
/// Provides additional implementation specific to Microsoft ASP.NET Core.
/// </content>
[CLSCompliant( false )]
public partial class ODataQueryOptionSettings
{
    private DefaultQueryConfigurations? queryConfig;

    /// <summary>
    /// Gets or sets the configured model metadata provider.
    /// </summary>
    /// <value>The configured <see cref="IModelMetadataProvider">model metadata provider</see>.</value>
    public IModelMetadataProvider? ModelMetadataProvider { get; set; }

    /// <summary>
    /// Gets or sets the OData query configurations.
    /// </summary>
    /// <value>The <see cref="QueryConfigurations">default OData query configurations</see>.</value>
    public DefaultQueryConfigurations QueryConfigurations
    {
        get => queryConfig ??= new();
        set => queryConfig = value;
    }
}