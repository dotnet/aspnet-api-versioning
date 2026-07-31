// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using Google.Protobuf;
using Google.Protobuf.Reflection;
using Grpc.Core;
using System.Collections;
using System.Globalization;

/// <summary>
/// Applies API version visibility to the fields of a protocol buffer message.
/// </summary>
internal static class MessageFields
{
    /// <summary>
    /// Clears the fields of a response message that are not visible in the requested API version.
    /// </summary>
    internal static void Filter( IAnnotation<FieldDescriptor, ApiVersionRange> annotations, object? value, ApiVersion apiVersion )
    {
        if ( value is IMessage message )
        {
            Visit( annotations, message, apiVersion, validate: false );
        }
    }

    /// <summary>
    /// Rejects a request message that supplies a field which is not visible in the requested API version.
    /// </summary>
    internal static void Validate( IAnnotation<FieldDescriptor, ApiVersionRange> annotations, object? value, ApiVersion apiVersion )
    {
        if ( value is IMessage message )
        {
            Visit( annotations, message, apiVersion, validate: true );
        }
    }

    private static void Visit(
        IAnnotation<FieldDescriptor, ApiVersionRange> annotations,
        IMessage message,
        ApiVersion apiVersion,
        bool validate )
    {
        var fields = message.Descriptor.Fields.InDeclarationOrder();

        for ( var i = 0; i < fields.Count; i++ )
        {
            var field = fields[i];

            if ( !annotations.IsVisible( field, apiVersion ) )
            {
                if ( !validate )
                {
                    field.Accessor.Clear( message );
                }
                else if ( IsSet( field, message ) )
                {
                    throw UnknownField( field );
                }

                continue;
            }

            if ( field.FieldType != FieldType.Message )
            {
                continue;
            }

            var value = field.Accessor.GetValue( message );

            // a repeated or map field yields a collection rather than a message, so its elements are visited
            // individually. the entry type of a map is synthetic and cannot be annotated, so only the values
            // of a map are visited
            if ( field.IsMap )
            {
                foreach ( var item in ( (IDictionary) value ).Values )
                {
                    if ( item is IMessage entry )
                    {
                        Visit( annotations, entry, apiVersion, validate );
                    }
                }
            }
            else if ( field.IsRepeated )
            {
                var items = (IList) value;

                for ( var j = 0; j < items.Count; j++ )
                {
                    if ( items[j] is IMessage element )
                    {
                        Visit( annotations, element, apiVersion, validate );
                    }
                }
            }
            else if ( value is IMessage nested )
            {
                Visit( annotations, nested, apiVersion, validate );
            }
        }
    }

    // a client is not told that a field exists in another API version. the field is reported the same way the
    // underlying parser reports a field it does not know about, which is what the client would have seen if the
    // field had never been defined
    private static RpcException UnknownField( FieldDescriptor field ) =>
        new( new Status( StatusCode.InvalidArgument, "Unknown field: " + field.JsonName ) );

    private static bool IsSet( FieldDescriptor field, IMessage message )
    {
        var accessor = field.Accessor;

        if ( field.IsMap || field.IsRepeated )
        {
            return accessor.GetValue( message ) is ICollection { Count: > 0 };
        }

        // a field with explicit presence records whether it was set, which is exact. a proto3 field with implicit
        // presence does not, so an explicitly supplied default cannot be distinguished from an absent value and
        // the best that can be done is to treat any non-default value as over-posted
        if ( field.HasPresence )
        {
            return accessor.HasValue( message );
        }

        return accessor.GetValue( message ) switch
        {
            null => false,
            string text => text.Length > 0,
            ByteString bytes => bytes.Length > 0,
            bool flag => flag,
            int number => number != 0,
            uint number => number != 0U,
            long number => number != 0L,
            ulong number => number != 0UL,
            float number => number != 0F,
            double number => number != 0D,
            Enum value => Convert.ToInt64( value, CultureInfo.InvariantCulture ) != 0L,
            _ => true,
        };
    }
}