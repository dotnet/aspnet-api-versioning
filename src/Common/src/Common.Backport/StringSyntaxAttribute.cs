// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace System.Diagnostics.CodeAnalysis;

// REF: https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/Diagnostics/CodeAnalysis/StringSyntaxAttribute.cs
//
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
[ExcludeFromCodeCoverage]
[AttributeUsage( AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false )]
internal sealed class StringSyntaxAttribute : Attribute
{
    public StringSyntaxAttribute( string syntax )
    {
        Syntax = syntax;
        Arguments = [];
    }

    public StringSyntaxAttribute( string syntax, params object?[] arguments )
    {
        Syntax = syntax;
        Arguments = arguments;
    }

    public string Syntax { get; }

    public object?[] Arguments { get; }
}