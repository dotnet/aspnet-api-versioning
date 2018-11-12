namespace Microsoft.OData.Edm
{
    using Microsoft.AspNet.OData;
#if !WEBAPI
    using Microsoft.AspNetCore.Mvc.ApiExplorer;
#endif
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reflection;
#if WEBAPI
    using System.Web.Http.Dispatcher;
#endif
    using static System.Globalization.CultureInfo;
    using static System.String;

    static class EdmExtensions
    {
        internal static Type GetClrType( this IEdmType edmType, IEdmModel edmModel )
        {
            Contract.Requires( edmType != null );
            Contract.Requires(edmModel != null );

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
            var annotationValue = edmModel.GetAnnotationValue<ClrTypeAnnotation>(element);
            if ( annotationValue != null )
            {
                return annotationValue.ClrType;
            }

            return null;
        }

        static Type DeriveFromWellKnowPrimitive( string edmFullName )
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
                    return Type.GetType( edmFullName.Replace( "Edm", "System" ), throwOnError: true );
                case "Edm.Duration":
                    return typeof( TimeSpan );
                case "Edm.Binary":
                    return typeof( byte[] );
                case "Edm.Geography":
                case "Edm.Geometry":
                    return Type.GetType( edmFullName.Replace( "Edm", "Microsoft.Spatial" ), throwOnError: true );
                case "Edm.Date":
                case "Edm.TimeOfDay":
                    return Type.GetType( edmFullName.Replace( "Edm", "Microsoft.OData.Edm" ), throwOnError: true );
            }

            return null;
        }
    }
}