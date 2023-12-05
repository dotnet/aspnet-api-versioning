// Copyright (c) .NET Foundation and contributors. All rights reserved.

// REF: https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/Runtime/CompilerServices/CallerArgumentExpressionAttribute.cs
namespace System.Runtime.CompilerServices
{
    [AttributeUsage( AttributeTargets.Parameter, AllowMultiple = false, Inherited = false )]
    internal sealed class CallerArgumentExpressionAttribute : Attribute
    {
        public CallerArgumentExpressionAttribute( string parameterName )
        {
            ParameterName = parameterName;
        }

        public string ParameterName { get; }
    }
}