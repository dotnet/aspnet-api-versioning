// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApiExplorer;

using System.Reflection;

internal sealed class PropertyParameterInfo( PropertyInfo property ) : ParameterInfo
{
    public override string? Name => property.Name;

    public override MemberInfo Member => property;

    public override Type ParameterType => property.PropertyType;

    public override bool HasDefaultValue => false;

    public override object[] GetCustomAttributes( bool inherit ) => property.GetCustomAttributes( inherit );

    public override object[] GetCustomAttributes( Type attributeType, bool inherit ) =>
        property.GetCustomAttributes( attributeType, inherit );
}