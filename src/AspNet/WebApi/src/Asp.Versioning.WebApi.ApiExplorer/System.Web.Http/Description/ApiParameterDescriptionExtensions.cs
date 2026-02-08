// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace System.Web.Http.Description;

using Asp.Versioning;
using System.Reflection;

internal static class ApiParameterDescriptionExtensions
{
    extension( ApiParameterDescription description )
    {
        internal IEnumerable<PropertyInfo> BindableProperties =>
            description.ParameterDescriptor.ParameterType.BindableProperties;

        internal bool CanConvertPropertiesFromString =>
            description.BindableProperties.All( p => TypeExtensions.get_CanConvertFromString( p.PropertyType ) );
    }
}