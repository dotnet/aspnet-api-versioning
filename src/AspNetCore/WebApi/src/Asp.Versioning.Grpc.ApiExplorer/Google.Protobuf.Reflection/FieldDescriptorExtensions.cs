// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace Google.Protobuf.Reflection;

using Asp.Versioning.Grpc;
using Google.Protobuf.WellKnownTypes;
using System.Globalization;
using static Google.Protobuf.Reflection.FieldType;
using static System.Globalization.CultureInfo;
using Type = System.Type;

internal static class FieldDescriptorExtensions
{
    extension( FieldDescriptor fieldDescriptor )
    {
        public Type ClrType
        {
            [RequiresDynamicCode( "Might not be available at runtime" )]
            get
            {
                if ( fieldDescriptor.IsMap )
                {
                    var mapFields = fieldDescriptor.MessageType.Fields.InFieldNumberOrder();
                    var valueType = mapFields[1].BaseClrType;

                    return typeof( IDictionary<,> ).MakeGenericType( typeof( string ), valueType );
                }
                else if ( fieldDescriptor.IsRepeated )
                {
                    return typeof( IList<> ).MakeGenericType( fieldDescriptor.BaseClrType );
                }

                return fieldDescriptor.BaseClrType;
            }
        }

        private Type BaseClrType => fieldDescriptor.FieldType switch
        {
            Double => typeof( double ),
            Float => typeof( float ),
            Int64 => typeof( long ),
            UInt64 => typeof( ulong ),
            Int32 => typeof( int ),
            Fixed64 => typeof( long ),
            Fixed32 => typeof( int ),
            Bool => typeof( bool ),
            String => typeof( string ),
            Bytes => typeof( string ),
            UInt32 => typeof( uint ),
            SFixed32 => typeof( int ),
            SFixed64 => typeof( long ),
            SInt32 => typeof( int ),
            SInt64 => typeof( long ),
            FieldType.Enum => fieldDescriptor.EnumType.ClrType,
            Message => fieldDescriptor.MessageType.ClrType,
            _ => throw fieldDescriptor.NewUnsupportedType(),
        };

        public object? ConvertValue( object? value ) => fieldDescriptor.FieldType switch
        {
            Double => Convert.ToDouble( value, InvariantCulture ),
            Float => Convert.ToSingle( value, InvariantCulture ),
            Int64 or SInt64 or SFixed64 => Convert.ToInt64( value, InvariantCulture ),
            UInt64 or Fixed64 => Convert.ToUInt64( value, InvariantCulture ),
            Int32 or SInt32 or SFixed32 => Convert.ToInt32( value, InvariantCulture ),
            UInt32 or Fixed32 => Convert.ToUInt32( value, InvariantCulture ),
            Bool => Convert.ToBoolean( value, InvariantCulture ),
            String => value,
            Bytes => FromBase64( value ),
            FieldType.Enum => fieldDescriptor.FromEnum( value ),
            Message => fieldDescriptor.FromMessage( value ),
            _ => throw fieldDescriptor.NewUnsupportedType(),
        };

        private InvalidOperationException NewUnsupportedType() => new( "Unsupported type: " + fieldDescriptor.FieldType );

        private int FromEnum( object? value )
        {
            if ( value is string s )
            {
                var enumValueDescriptor = ( int.TryParse( s, NumberStyles.Integer, InvariantCulture, out var i )
                    ? fieldDescriptor.EnumType.FindValueByNumber( i )
                    : fieldDescriptor.EnumType.FindValueByName( s ) ) ?? throw new InvalidOperationException( $"Invalid value '{s}' for enum type {fieldDescriptor.EnumType.Name}." );
                return enumValueDescriptor.Number;
            }

            throw new InvalidOperationException( "String required to convert to enum." );
        }

        private object? FromMessage( object? value )
        {
            if ( !fieldDescriptor.MessageType.IsWellKnownType )
            {
                throw fieldDescriptor.NewUnsupportedType();
            }

            if ( fieldDescriptor.MessageType.IsWrapperType )
            {
                if ( value is null )
                {
                    return default;
                }

                return fieldDescriptor.MessageType.FindFieldByName( "value" ).ConvertValue( value );
            }
            else if ( fieldDescriptor.MessageType.FullName == FieldMask.Descriptor.FullName )
            {
                return FieldMask.FromString( (string) value! );
            }
            else if ( fieldDescriptor.MessageType.FullName == Duration.Descriptor.FullName )
            {
                return Time.FromSeconds( ( (string) value! ).AsSpan() );
            }
            else if ( fieldDescriptor.MessageType.FullName == Timestamp.Descriptor.FullName )
            {
                return Time.FromRfc3339( ( (string) value! ).AsSpan() );
            }
            else
            {
                throw fieldDescriptor.NewUnsupportedType();
            }
        }
    }

    private static ByteString FromBase64( object? value )
    {
        if ( value is string bytes )
        {
            return ByteString.FromBase64( bytes );
        }

        throw new InvalidOperationException( "Base64 encoded string required to convert to bytes." );
    }
}