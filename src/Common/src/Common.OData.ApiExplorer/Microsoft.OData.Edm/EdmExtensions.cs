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
        "Edm.String" => typeof( string ),
        "Edm.Byte" => typeof( byte ),
        "Edm.SByte" => typeof( sbyte ),
        "Edm.Int32" => typeof( int ),
        "Edm.Int64" => typeof( long ),
        "Edm.Int16" => typeof( short ),
        "Edm.Double" => typeof( double ),
        "Edm.Single" => typeof( float ),
        "Edm.Boolean" => typeof( bool ),
        "Edm.Decimal" => typeof( decimal ),
        "Edm.DateTime" => typeof( DateTime ),
        "Edm.DateTimeOffset" => typeof( DateTimeOffset ),
        "Edm.Guid" => typeof( Guid ),
        "Edm.Duration" => typeof( TimeSpan ),
        "Edm.Binary" => typeof( byte[] ),
        "Edm.Geography" => typeof( Microsoft.Spatial.Geography ),
        "Edm.Geometry" => typeof( Microsoft.Spatial.Geometry ),
#if NETFRAMEWORK
        "Edm.Date" => typeof( Microsoft.OData.Edm.Date ),
        "Edm.TimeOfDay" => typeof( Microsoft.OData.Edm.TimeOfDay ),
#else
        "Edm.Date" => typeof( DateOnly ),
        "Edm.TimeOfDay" => typeof( TimeSpan ),
#endif
        _ => default,
    };
}