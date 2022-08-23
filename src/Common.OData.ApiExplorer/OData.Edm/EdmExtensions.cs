namespace Microsoft.OData.Edm
{
    using Microsoft.AspNet.OData;
#if !WEBAPI
    using Microsoft.AspNetCore.Mvc.ApiExplorer;
#endif
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;

    static class EdmExtensions
    {
        const bool ThrowOnError = true;

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

        static Type? DeriveFromWellKnowPrimitive( string edmFullName ) => edmFullName switch
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
#if WEBAPI
        static string Requalify( string edmFullName, string @namespace ) => @namespace + edmFullName.Substring( 3 );
#else
        static string Requalify( string edmFullName, string @namespace ) => string.Concat( @namespace.AsSpan(), edmFullName.AsSpan().Slice( 3 ) );
#endif

        static Type? GetTypeFromAssembly( string edmFullName, string assemblyName )
        {
            var typeName = Requalify( edmFullName, assemblyName ) + "," + assemblyName;
            return Type.GetType( typeName, ThrowOnError );
        }
    }
}