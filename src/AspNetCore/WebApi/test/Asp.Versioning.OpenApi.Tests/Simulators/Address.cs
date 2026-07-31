// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OpenApi.Simulators;

/// <summary>
/// Represents an address.
/// </summary>
public class Address
{
    /// <summary>
    /// Gets or sets the street.
    /// </summary>
    public string Street { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the country.
    /// </summary>
    [VisibleInApiVersion( "2.0" )]
    public string Country { get; set; } = string.Empty;
}