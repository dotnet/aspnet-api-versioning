// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0079
#pragma warning disable IDE0130

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

internal static partial class TypeExtensions
{
    private static Type? odataRoutingAttributeType;
    private static Type? metadataController;
    private static Type? delta;
    private static Type? odataPath;
    private static Type? odataQueryOptions;
    private static Type? odataActionParameters;

    extension( Type type )
    {
        internal bool IsODataController => type.UsingOData;

        internal bool IsMetadataController
        {
            get
            {
                metadataController ??= typeof( MetadataController );
                return metadataController.IsAssignableFrom( type );
            }
        }

        internal bool IsODataPath
        {
            get
            {
                odataPath ??= typeof( ODataPath );
                return odataPath.IsAssignableFrom( type );
            }
        }

        internal bool IsODataQueryOptions
        {
            get
            {
                odataQueryOptions ??= typeof( ODataQueryOptions );
                return odataQueryOptions.IsAssignableFrom( type );
            }
        }

        internal bool IsODataActionParameters
        {
            get
            {
                odataActionParameters ??= typeof( ODataActionParameters );
                return odataActionParameters.IsAssignableFrom( type );
            }
        }

        internal bool IsDelta
        {
            get
            {
                delta ??= typeof( IDelta );
                return delta.IsAssignableFrom( type );
            }
        }
    }

    extension( MemberInfo member )
    {
        private bool UsingOData
        {
            get
            {
                odataRoutingAttributeType ??= typeof( ODataRoutingAttribute );
                return Attribute.IsDefined( member, odataRoutingAttributeType );
            }
        }
    }
}