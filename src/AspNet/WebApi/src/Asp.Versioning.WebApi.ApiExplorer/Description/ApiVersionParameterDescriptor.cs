﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Description;

using System.Web.Http.Controllers;

/// <summary>
/// Represents a parameter descriptor for an <see cref="ApiVersion">API version</see>.
/// </summary>
public sealed class ApiVersionParameterDescriptor : HttpParameterDescriptor
{
    private readonly string parameterName;
    private readonly object defaultValue;
    private readonly bool optional;

    internal ApiVersionParameterDescriptor( string parameterName, object defaultValue, bool optional = false, bool fromPath = false )
    {
        this.parameterName = parameterName;
        this.defaultValue = defaultValue;
        this.optional = optional;
        FromPath = fromPath;
    }

    /// <inheritdoc />
    public override string ParameterName => parameterName;

    /// <inheritdoc />
    public override Type ParameterType => typeof( string );

    /// <inheritdoc />
    public override object DefaultValue => defaultValue;

    /// <inheritdoc />
    public override bool IsOptional => optional;

    /// <summary>
    /// Gets a value indicating whether the parameter descriptor represents a URL segment.
    /// </summary>
    /// <value>True if the parameter descriptor represents a URL segment; otherwise, false.
    /// The default value is <c>false</c>.</value>
    public bool FromPath { get; }
}