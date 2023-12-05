// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using Microsoft.AspNetCore.Mvc.Controllers;
using System.Collections;
using System.Reflection;

internal sealed class FilteredControllerTypes : ControllerFeatureProvider, ICollection<Type>
{
    private readonly HashSet<Type> controllerTypes = [];

    protected override bool IsController( TypeInfo typeInfo ) => base.IsController( typeInfo ) && controllerTypes.Contains( typeInfo );

    public int Count => controllerTypes.Count;

    public bool IsReadOnly => false;

    public void Add( Type item ) => controllerTypes.Add( item );

    public void Clear() => controllerTypes.Clear();

    public bool Contains( Type item ) => controllerTypes.Contains( item );

    public void CopyTo( Type[] array, int arrayIndex ) => controllerTypes.CopyTo( array, arrayIndex );

    public IEnumerator<Type> GetEnumerator() => controllerTypes.GetEnumerator();

    public bool Remove( Type item ) => controllerTypes.Remove( item );

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}