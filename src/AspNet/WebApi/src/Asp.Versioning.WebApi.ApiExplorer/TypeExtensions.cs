// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace System;

using System.Reflection;
using static System.Reflection.BindingFlags;

internal static class TypeExtensions
{
    extension( Type type )
    {
        internal Type[]? GetTypeArgumentsIfMatch( Type matchingOpenType )
        {
            if ( !type.IsGenericType )
            {
                return null;
            }

            var openType = type.GetGenericTypeDefinition();

            return ( matchingOpenType == openType ) ? type.GetGenericArguments() : null;
        }

        internal IEnumerable<PropertyInfo> BindableProperties =>
            type.GetProperties( Instance | Public ).Where( p => p.GetGetMethod() != null && p.GetSetMethod() != null );

        internal Type[]? GetGenericBinderTypeArgs( Type modelType )
        {
            if ( !modelType.IsGenericType || modelType.IsGenericTypeDefinition )
            {
                return null;
            }

            var modelTypeArguments = modelType.GetGenericArguments();

            if ( modelTypeArguments.Length != type.GetGenericArguments().Length )
            {
                return null;
            }

            return modelTypeArguments;
        }
    }
}