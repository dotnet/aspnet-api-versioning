namespace Microsoft.OData.Edm
{
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
        internal static Type GetClrType( this IEdmType edmType, IAssembliesResolver assembliesResolver )
        {
            Contract.Requires( edmType != null );
            Contract.Requires( assembliesResolver != null );

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

            using ( var matchingTypes = GetMatchingTypes( typeName, assembliesResolver ).GetEnumerator() )
            {
                if ( matchingTypes.MoveNext() )
                {
                    type = matchingTypes.Current;

                    if ( !matchingTypes.MoveNext() )
                    {
                        return type;
                    }
                }
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

        static string EdmFullName( this Type clrType ) => Format( InvariantCulture, "{0}.{1}", clrType.Namespace, clrType.MangleClrTypeName() );

        static string MangleClrTypeName( this Type type )
        {
            Contract.Requires( type != null );
            Contract.Ensures( !IsNullOrEmpty( Contract.Result<string>() ) );

            if ( !type.IsGenericType )
            {
                return type.Name;
            }

            var typeName = type.Name.Replace( '`', '_' );
            var typeArgNames = Join( "_", type.GetGenericArguments().Select( t => t.MangleClrTypeName() ) );

            return Format( InvariantCulture, "{0}Of{1}", typeName, typeArgNames );
        }

        static IEnumerable<Type> GetMatchingTypes( string edmFullName, IAssembliesResolver assembliesResolver ) =>
            assembliesResolver.LoadedTypes().Where( t => t.IsPublic && t.EdmFullName() == edmFullName );

        static IEnumerable<Type> LoadedTypes( this IAssembliesResolver assembliesResolver )
        {
            var loadedTypes = new List<Type>();
            var assemblies = assembliesResolver.GetAssemblies();

            foreach ( var assembly in assemblies.Where( a => a?.IsDynamic == false ) )
            {
                var exportedTypes = default( IEnumerable<Type> );

                try
                {
                    exportedTypes = assembly.ExportedTypes;
                }
                catch ( ReflectionTypeLoadException ex )
                {
                    exportedTypes = ex.Types;
                }
                catch
                {
                    continue;
                }

                if ( exportedTypes != null )
                {
                    loadedTypes.AddRange( exportedTypes );
                }
            }

            return loadedTypes;
        }
    }
}