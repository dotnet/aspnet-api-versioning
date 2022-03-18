// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using Microsoft.AspNetCore.Mvc.ApplicationParts;
using System.Reflection;

internal sealed class TestApplicationPart : ApplicationPart, IApplicationPartTypeProvider
{
    public TestApplicationPart() => Types = Enumerable.Empty<TypeInfo>();

    public TestApplicationPart( params TypeInfo[] types ) => Types = types;

    public TestApplicationPart( IEnumerable<TypeInfo> types ) => Types = types;

    public TestApplicationPart( IEnumerable<Type> types ) : this( types.Select( t => t.GetTypeInfo() ) ) { }

    public TestApplicationPart( params Type[] types ) : this( types.Select( t => t.GetTypeInfo() ) ) { }

    public override string Name => "Test Part";

    public IEnumerable<TypeInfo> Types { get; }
}