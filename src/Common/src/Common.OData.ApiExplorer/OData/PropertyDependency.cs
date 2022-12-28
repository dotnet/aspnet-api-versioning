// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData;

using System.Reflection.Emit;

internal sealed class PropertyDependency
{
    internal PropertyDependency(
        EdmTypeKey dependentOnTypeKey,
        bool isCollection,
        string propertyName,
        IEnumerable<CustomAttributeBuilder> customAttributes )
    {
        DependentOnTypeKey = dependentOnTypeKey;
        PropertyName = propertyName;
        CustomAttributes = customAttributes.ToArray();
        IsCollection = isCollection;
    }

    internal TypeBuilder? DependentType { get; set; }

    internal EdmTypeKey DependentOnTypeKey { get; }

    internal string PropertyName { get; }

    internal bool IsCollection { get; }

    internal IReadOnlyList<CustomAttributeBuilder> CustomAttributes { get; }
}