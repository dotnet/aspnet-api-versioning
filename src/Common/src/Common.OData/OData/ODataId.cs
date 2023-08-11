﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData;

#if NETFRAMEWORK
using Newtonsoft.Json;
#else
using JsonProperty = System.Text.Json.Serialization.JsonPropertyNameAttribute;
#endif

/// <summary>
/// Represents an OData identifier specified in the body of a POST or PUT OData relationship reference request.
/// </summary>
public class ODataId
{
    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    /// <value>The <see cref="Uri">URL</see> representing the related entity identifier.</value>
    [JsonProperty( "@odata.id" )]
#if NETFRAMEWORK
    public Uri Value { get; set; } = default!;
#else
    public required Uri Value { get; set; }
#endif
}