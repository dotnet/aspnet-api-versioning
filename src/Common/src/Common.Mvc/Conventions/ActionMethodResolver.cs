// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

#if !NETFRAMEWORK
using Microsoft.AspNetCore.Mvc;
#endif
using System.Globalization;
using System.Reflection;
#if NETFRAMEWORK
using System.Web.Http;
#endif

internal static class ActionMethodResolver
{
    internal static MethodInfo Resolve( Type controllerType, string methodName, Type[] argumentTypes )
    {
        var methods = controllerType.GetRuntimeMethods().Where( m => m.Name == methodName && IsAction( m ) ).ToArray();

        switch ( methods.Length )
        {
            case 0:
                throw new MissingMethodException( string.Format( CultureInfo.CurrentCulture, MvcFormat.ActionMethodNotFound, methodName ) );
            case 1:
                return methods[0];
        }

        argumentTypes ??= Type.EmptyTypes;
        methods = methods.Where( m => SignatureMatches( m, argumentTypes ) ).ToArray();

        if ( methods.Length == 1 )
        {
            return methods[0];
        }

        throw new AmbiguousMatchException( string.Format( CultureInfo.CurrentCulture, MvcFormat.AmbiguousActionMethod, methodName ) );
    }

    private static bool IsAction( MethodInfo method ) =>
        method.IsPublic && !method.IsStatic && method.GetCustomAttribute<NonActionAttribute>() == null;

    private static bool SignatureMatches( MethodInfo method, Type[] argumentTypes ) =>
        method.GetParameters().Select( p => p.ParameterType ).ToArray().SequenceEqual( argumentTypes );
}