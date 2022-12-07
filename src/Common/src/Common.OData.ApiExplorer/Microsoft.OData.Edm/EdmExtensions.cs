// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Microsoft.OData.Edm;

#if NETFRAMEWORK
using Microsoft.AspNet.OData;
#else
using Microsoft.OData.ModelBuilder;
#endif
using System.Runtime.CompilerServices;

internal static class EdmExtensions
{
    private const bool ThrowOnError = true;

    internal static Type? GetClrType( this IEdmType edmType, IEdmModel edmModel )
    {
        if ( edmType is not IEdmSchemaType schemaType )
        {
            return null;
        }

        var typeName = schemaType.FullName();

        if ( DeriveFromWellKnowPrimitive( typeName ) is Type type )
        {
            return type;
        }

        var annotationValue = edmModel.GetAnnotationValue<ClrTypeAnnotation>( schemaType );

        if ( annotationValue != null )
        {
            return annotationValue.ClrType;
        }

        return null;
    }

    private static Type? DeriveFromWellKnowPrimitive( string edmFullName ) => edmFullName switch
    {
        "Edm.String" or "Edm.Byte" or "Edm.SByte" or "Edm.Int16" or "Edm.Int32" or "Edm.Int64" or
        "Edm.Double" or "Edm.Single" or "Edm.Boolean" or "Edm.Decimal" or "Edm.DateTime" or "Edm.DateTimeOffset" or
        "Edm.Guid" => Type.GetType( Requalify( edmFullName, "System" ), ThrowOnError ),
        "Edm.Duration" => typeof( TimeSpan ),
        "Edm.Binary" => typeof( byte[] ),
        "Edm.Geography" or "Edm.Geometry" => GetTypeFromAssembly( edmFullName, "Microsoft.Spatial" ),
        "Edm.Date" or "Edm.TimeOfDay" => GetTypeFromAssembly( edmFullName, "Microsoft.OData.Edm" ),
        _ => null,
    };

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
#if NETFRAMEWORK
    private static string Requalify( string edmFullName, string @namespace ) => @namespace + edmFullName.Substring( 3 );
#else
    private static string Requalify( string edmFullName, string @namespace ) => string.Concat( @namespace.AsSpan(), edmFullName.AsSpan()[3..] );
#endif

    private static Type? GetTypeFromAssembly( string edmFullName, string assemblyName )
    {
        var typeName = Requalify( edmFullName, assemblyName ) + "," + assemblyName;
        return Type.GetType( typeName, ThrowOnError );
    }
}