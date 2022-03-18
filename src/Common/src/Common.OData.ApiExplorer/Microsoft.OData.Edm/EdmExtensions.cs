// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Microsoft.OData.Edm;

#if NETFRAMEWORK
using Microsoft.AspNet.OData;
#else
using Microsoft.OData.ModelBuilder;
#endif
using static System.StringComparison;

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
        "Edm.Guid" => Type.GetType( edmFullName.Replace( "Edm", "System", Ordinal ), ThrowOnError ),
        "Edm.Duration" => typeof( TimeSpan ),
        "Edm.Binary" => typeof( byte[] ),
        "Edm.Geography" or "Edm.Geometry" => Type.GetType( edmFullName.Replace( "Edm", "Microsoft.Spatial", Ordinal ), ThrowOnError ),
        "Edm.Date" or "Edm.TimeOfDay" => Type.GetType( edmFullName.Replace( "Edm", "Microsoft.OData.Edm", Ordinal ), ThrowOnError ),
        _ => null,
    };
}