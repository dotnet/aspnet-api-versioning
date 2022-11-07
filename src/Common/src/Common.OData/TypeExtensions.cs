// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace System;

#if NETFRAMEWORK
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing;
#else
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.OData.UriParser;
using ODataRoutingAttribute = Microsoft.AspNetCore.OData.Routing.Attributes.ODataAttributeRoutingAttribute;
#endif
using System.Reflection;
using System.Runtime.CompilerServices;

internal static partial class TypeExtensions
{
    private static Type? odataRoutingAttributeType;
    private static Type? metadataController;
    private static Type? delta;
    private static Type? odataPath;
    private static Type? odataQueryOptions;
    private static Type? odataActionParameters;

    internal static bool IsODataController( this Type controllerType ) => controllerType.UsingOData();

    internal static bool IsMetadataController( this Type controllerType )
    {
        metadataController ??= typeof( MetadataController );
        return metadataController.IsAssignableFrom( controllerType );
    }

    internal static bool IsODataPath( this Type type )
    {
        odataPath ??= typeof( ODataPath );
        return odataPath.IsAssignableFrom( type );
    }

    internal static bool IsODataQueryOptions( this Type type )
    {
        odataQueryOptions ??= typeof( ODataQueryOptions );
        return odataQueryOptions.IsAssignableFrom( type );
    }

    internal static bool IsODataActionParameters( this Type type )
    {
        odataActionParameters ??= typeof( ODataActionParameters );
        return odataActionParameters.IsAssignableFrom( type );
    }

    internal static bool IsDelta( this Type type )
    {
        delta ??= typeof( IDelta );
        return delta.IsAssignableFrom( type );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static bool UsingOData( this MemberInfo member )
    {
        odataRoutingAttributeType ??= typeof( ODataRoutingAttribute );
        return Attribute.IsDefined( member, odataRoutingAttributeType );
    }
}