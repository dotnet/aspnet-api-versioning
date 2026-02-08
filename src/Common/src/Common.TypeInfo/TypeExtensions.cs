// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using System.ComponentModel;

internal static partial class TypeExtensions
{
    extension( Type type )
    {
        internal bool IsSimpleType
        {
            get
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
        }

        internal bool IsSimpleUnderlyingType
        {
            get
            {
                var underlyingType = Nullable.GetUnderlyingType( type );

                if ( underlyingType != null )
                {
                    type = underlyingType;
                }

                return type.IsSimpleType;
            }
        }

        internal bool HasStringConverter => TypeDescriptor.GetConverter( type ).CanConvertFrom( typeof( string ) );

        internal bool CanConvertFromString => type.IsSimpleUnderlyingType || type.HasStringConverter;
    }
}