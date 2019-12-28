namespace Microsoft.OData.Edm
{
    using Microsoft.AspNet.OData;
#if !WEBAPI
    using Microsoft.AspNetCore.Mvc.ApiExplorer;
#endif
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
#if WEBAPI
    using System.Web.Http.Dispatcher;
#endif
    using static System.Globalization.CultureInfo;
    using static System.String;
#if !WEBAPI
    using static System.StringComparison;
#endif

    static class EdmExtensions
    {
        internal static Type? GetClrType( this IEdmType edmType, IEdmModel edmModel )
        {
            if ( !( edmType is IEdmSchemaType schemaType ) )
            {
                return null;
            }

            var typeName = schemaType.FullName();
            var type = DeriveFromWellKnowPrimitive( typeName );

            if ( type != null )
            {
                return type;
            }

            var element = (IEdmSchemaType) edmType;
            var annotationValue = edmModel.GetAnnotationValue<ClrTypeAnnotation>( element );

            if ( annotationValue != null )
            {
                return annotationValue.ClrType;
            }

            return null;
        }

        static Type? DeriveFromWellKnowPrimitive( string edmFullName )
        {
            switch ( edmFullName )
            {
                case "Edm.String":
                case "Edm.Byte":
                case "Edm.SByte":
                case "Edm.Int16":
                case "Edm.Int32":
                case "Edm.Int64":
                case "Edm.Double":
                case "Edm.Single":
                case "Edm.Boolean":
                case "Edm.Decimal":
                case "Edm.DateTime":
                case "Edm.DateTimeOffset":
                case "Edm.Guid":
#if WEBAPI
                    return Type.GetType( edmFullName.Replace( "Edm", "System" ), throwOnError: true );
#else
                    return Type.GetType( edmFullName.Replace( "Edm", "System", Ordinal ), throwOnError: true );
#endif
                case "Edm.Duration":
                    return typeof( TimeSpan );
                case "Edm.Binary":
                    return typeof( byte[] );
                case "Edm.Geography":
                case "Edm.Geometry":
#if WEBAPI
                    return Type.GetType( edmFullName.Replace( "Edm", "Microsoft.Spatial" ), throwOnError: true );
#else
                    return Type.GetType( edmFullName.Replace( "Edm", "Microsoft.Spatial", Ordinal ), throwOnError: true );
#endif
                case "Edm.Date":
                case "Edm.TimeOfDay":
#if WEBAPI
                    return Type.GetType( edmFullName.Replace( "Edm", "Microsoft.OData.Edm" ), throwOnError: true );
#else
                    return Type.GetType( edmFullName.Replace( "Edm", "Microsoft.OData.Edm", Ordinal ), throwOnError: true );
#endif
            }

            return null;
        }
    }
}