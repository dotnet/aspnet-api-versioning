// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

using Microsoft.AspNet.OData.Query;

/// <content>
/// Provides additional implementation specific to Microsoft ASP.NET Web API.
/// </content>
public partial class ODataQueryOptionSettings
{
    private DefaultQuerySettings? querySettings;

    /// <summary>
    /// Gets or sets the default OData query settings.
    /// </summary>
    /// <value>The <see cref="DefaultQuerySettings">default OData query settings</see>.</value>
    public DefaultQuerySettings DefaultQuerySettings
    {
        get => querySettings ??= new();
        set => querySettings = value;
    }
}