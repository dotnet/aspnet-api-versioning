// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Description;

using System.Collections.ObjectModel;
using System.Web.Http;
using System.Web.Http.Controllers;

internal sealed class ODataModelBoundParameterDescriptor : HttpParameterDescriptor
{
    private readonly HttpParameterDescriptor decorated;

    internal ODataModelBoundParameterDescriptor( HttpParameterDescriptor decorated, Type parameterType )
    {
        this.decorated = decorated;
        ParameterType = parameterType;
    }

    public override object DefaultValue => decorated.DefaultValue;

    public override Collection<T> GetCustomAttributes<T>() => decorated.GetCustomAttributes<T>();

    public override bool Equals( object obj ) => decorated.Equals( obj );

    public override int GetHashCode() => decorated.GetHashCode();

    public override bool IsOptional => decorated.IsOptional;

    public override ParameterBindingAttribute ParameterBinderAttribute
    {
        get => decorated.ParameterBinderAttribute;
        set => decorated.ParameterBinderAttribute = value;
    }

    public override string ParameterName => decorated.ParameterName;

    public override Type ParameterType { get; }

    public override string Prefix => decorated.Prefix;

    public override string ToString() => decorated.ToString();
}