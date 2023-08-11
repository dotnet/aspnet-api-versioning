// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData;

#if NETFRAMEWORK
using Newtonsoft.Json;
#else
using JsonProperty = System.Text.Json.Serialization.JsonPropertyNameAttribute;
#endif

/// <summary>
/// Represents a placeholder for describing OData responses that are represented as an
/// object with a single name/value pair whose name is "value".
/// </summary>
/// <typeparam name="T">The <see cref="Type">type</see> of content in the "value".</typeparam>
public class ODataValue<T>
{
    /// <summary>
    /// Gets or sets the OData response content in the "value".
    /// </summary>
    /// <value>The response content within "value".</value>
    [JsonProperty( "value" )]
#if NETFRAMEWORK
    public T Value { get; set; } = default!;
#else
    public required T Value { get; set; }
#endif
}