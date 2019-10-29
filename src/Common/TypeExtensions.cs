#if WEBAPI
namespace Microsoft.Web.Http
#else
namespace Microsoft.AspNetCore.Mvc
#endif
{
    using System;
    using static System.ComponentModel.TypeDescriptor;
    using static System.Nullable;

    static partial class TypeExtensions
    {
        internal static bool IsSimpleType( this Type type )
        {
#if WEBAPI
            return type.IsPrimitive ||
#else
            return type.IsPrimitive() ||
#endif
                   type.Equals( typeof( string ) ) ||
                   type.Equals( typeof( DateTime ) ) ||
                   type.Equals( typeof( decimal ) ) ||
                   type.Equals( typeof( Guid ) ) ||
                   type.Equals( typeof( DateTimeOffset ) ) ||
                   type.Equals( typeof( TimeSpan ) );
        }

        internal static bool IsSimpleUnderlyingType( this Type type )
        {
            var underlyingType = GetUnderlyingType( type );

            if ( underlyingType != null )
            {
                type = underlyingType;
            }

            return type.IsSimpleType();
        }

        internal static bool HasStringConverter( this Type type ) => GetConverter( type ).CanConvertFrom( typeof( string ) );

        internal static bool CanConvertFromString( this Type type ) => type.IsSimpleUnderlyingType() || type.HasStringConverter();
    }
}