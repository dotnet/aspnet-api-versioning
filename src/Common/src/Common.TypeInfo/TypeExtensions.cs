// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using System.ComponentModel;

internal static partial class TypeExtensions
{
    internal static bool IsSimpleType( this Type type )
    {
#if NETFRAMEWORK
        return type.IsPrimitive ||
#else
        return type.IsPrimitive() ||
#endif
        type.Equals( typeof( string ) ) ||
        type.Equals( typeof( decimal ) ) ||
        type.Equals( typeof( DateTime ) ) ||
        type.Equals( typeof( TimeSpan ) ) ||
        type.Equals( typeof( DateTimeOffset ) ) ||
#if !NETFRAMEWORK
        type.Equals( typeof( DateOnly ) ) ||
        type.Equals( typeof( TimeOnly ) ) ||
#endif
        type.Equals( typeof( Guid ) );
    }

    internal static bool IsSimpleUnderlyingType( this Type type )
    {
        var underlyingType = Nullable.GetUnderlyingType( type );

        if ( underlyingType != null )
        {
            type = underlyingType;
        }

        return type.IsSimpleType();
    }

    internal static bool HasStringConverter( this Type type ) =>
        TypeDescriptor.GetConverter( type ).CanConvertFrom( typeof( string ) );

    internal static bool CanConvertFromString( this Type type ) => type.IsSimpleUnderlyingType() || type.HasStringConverter();
}