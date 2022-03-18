// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

using System.Web.Http.Controllers;

internal sealed class ODataQueryOptionParameterDescriptor : HttpParameterDescriptor
{
    private string prefix = "$";

    internal ODataQueryOptionParameterDescriptor(
        string name,
        Type type,
        object? defaultValue,
        bool optional = true )
    {
        ParameterName = name;
        ParameterType = type;
        DefaultValue = defaultValue;
        IsOptional = optional;
    }

    public override string ParameterName { get; }

    public override Type ParameterType { get; }

    public override object? DefaultValue { get; }

    public override string Prefix => prefix;

    public override bool IsOptional { get; }

    internal void SetPrefix( string value ) => prefix = value ?? string.Empty;
}