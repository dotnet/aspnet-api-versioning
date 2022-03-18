// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace System.Web.Http.Description;

using Asp.Versioning;
using System.Reflection;

internal static class ApiParameterDescriptionExtensions
{
    internal static IEnumerable<PropertyInfo> GetBindableProperties( this ApiParameterDescription description ) =>
        description.ParameterDescriptor.ParameterType.GetBindableProperties();

    internal static bool CanConvertPropertiesFromString( this ApiParameterDescription description ) =>
        description.GetBindableProperties().All( p => p.PropertyType.CanConvertFromString() );
}