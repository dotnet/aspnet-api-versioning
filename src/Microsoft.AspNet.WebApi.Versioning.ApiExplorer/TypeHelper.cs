namespace Microsoft.Web.Http
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using static System.ComponentModel.TypeDescriptor;
    using static System.Reflection.BindingFlags;

    static class TypeHelper
    {
        internal static Type[] GetTypeArgumentsIfMatch( Type closedType, Type matchingOpenType )
        {
            if ( !closedType.IsGenericType )
            {
                return null;
            }

            var openType = closedType.GetGenericTypeDefinition();

            return ( matchingOpenType == openType ) ? closedType.GetGenericArguments() : null;
        }

        internal static bool CanConvertFromString( Type type ) => IsSimpleUnderlyingType( type ) || HasStringConverter( type );

        internal static bool IsSimpleUnderlyingType( Type type )
        {
            var underlyingType = Nullable.GetUnderlyingType( type );

            if ( underlyingType != null )
            {
                type = underlyingType;
            }

            return IsSimpleType( type );
        }

        internal static bool HasStringConverter( Type type ) => GetConverter( type ).CanConvertFrom( typeof( string ) );

        internal static bool IsSimpleType( Type type )
        {
            return type.IsPrimitive ||
                   type.Equals( typeof( string ) ) ||
                   type.Equals( typeof( DateTime ) ) ||
                   type.Equals( typeof( decimal ) ) ||
                   type.Equals( typeof( Guid ) ) ||
                   type.Equals( typeof( DateTimeOffset ) ) ||
                   type.Equals( typeof( TimeSpan ) );
        }

        internal static IEnumerable<PropertyInfo> GetBindableProperties( this Type type ) =>
            type.GetProperties( Instance | Public ).Where( p => p.GetGetMethod() != null && p.GetSetMethod() != null );

        internal static Type[] GetGenericBinderTypeArgs( this Type supportedInterfaceType, Type modelType )
        {
            if ( !modelType.IsGenericType || modelType.IsGenericTypeDefinition )
            {
                // not a closed generic type
                return null;
            }

            var modelTypeArguments = modelType.GetGenericArguments();

            if ( modelTypeArguments.Length != supportedInterfaceType.GetGenericArguments().Length )
            {
                // wrong number of generic type arguments
                return null;
            }

            return modelTypeArguments;
        }
    }
}