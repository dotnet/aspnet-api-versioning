// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0060
#pragma warning disable IDE0079
#pragma warning disable SA1402
#pragma warning disable SA1649

// REF: https://github.com/dotnet/runtime/blob/1c8d37af80667daffb3cb80ce0fe915621e8f039/src/libraries/System.Private.CoreLib/src/System/Diagnostics/CodeAnalysis/NullableAttributes.cs
//
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
namespace System.Diagnostics.CodeAnalysis;

[AttributeUsage( AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property, Inherited = false )]
internal sealed class AllowNullAttribute : Attribute { }

[AttributeUsage( AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property, Inherited = false )]
internal sealed class DisallowNullAttribute : Attribute { }

[AttributeUsage( AttributeTargets.Method, Inherited = false )]
internal sealed class DoesNotReturnAttribute : Attribute { }

[AttributeUsage( AttributeTargets.Parameter )]
internal sealed class DoesNotReturnIfAttribute : Attribute
{
    public DoesNotReturnIfAttribute( bool parameterValue ) { }

    public bool ParameterValue => default;
}

[AttributeUsage( AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Constructor | AttributeTargets.Event | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Struct, Inherited = false, AllowMultiple = false )]
internal sealed class ExcludeFromCodeCoverageAttribute : Attribute { }

[AttributeUsage( AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue, Inherited = false )]
internal sealed class MaybeNullAttribute : Attribute { }

[AttributeUsage( AttributeTargets.Parameter )]
internal sealed class MaybeNullWhenAttribute : Attribute
{
    public MaybeNullWhenAttribute( bool returnValue ) { }

    public bool ReturnValue => default;
}

[AttributeUsage( AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue, Inherited = false )]
internal sealed class NotNullAttribute : Attribute { }

[AttributeUsage( AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue, AllowMultiple = true, Inherited = false )]
internal sealed class NotNullIfNotNullAttribute : Attribute
{
    public NotNullIfNotNullAttribute( string parameterName ) { }

    public string ParameterName => default!;
}

[AttributeUsage( AttributeTargets.Parameter )]
internal sealed class NotNullWhenAttribute : Attribute
{
    public NotNullWhenAttribute( bool returnValue ) { }

    public bool ReturnValue => default;
}