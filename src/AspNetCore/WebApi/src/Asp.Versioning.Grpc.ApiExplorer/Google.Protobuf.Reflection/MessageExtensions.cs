// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace Google.Protobuf.Reflection;

using Asp.Versioning;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Primitives;
using System.Collections;

internal static class MessageExtensions
{
    extension( IMessage message )
    {
        public void SetValue( List<FieldDescriptor> pathDescriptors, object? values )
        {
            for ( var i = 0; i < pathDescriptors.Count; i++ )
            {
                var last = i == pathDescriptors.Count - 1;
                var field = pathDescriptors[i];

                if ( last )
                {
                    message.SetValue( field, values );
                }
                else
                {
                    if ( field.Accessor.GetValue( message ) is not IMessage value )
                    {
                        value = (IMessage) Activator.CreateInstance( field.MessageType.ClrType )!;
                        field.Accessor.SetValue( message, value );
                    }

                    message = value;
                }
            }
        }

        public void SetValue( FieldDescriptor field, object? values )
        {
            if ( field.IsMap )
            {
                var map = (IDictionary) field.Accessor.GetValue( message );

                if ( values is IDictionary entries )
                {
                    foreach ( DictionaryEntry entry in entries )
                    {
                        map[entry.Key] = entry.Value;
                    }
                }
                else
                {
                    throw new InvalidOperationException( SR.MapRequiresRepeating );
                }
            }
            else if ( field.IsRepeated )
            {
                var list = (IList) field.Accessor.GetValue( message );

                if ( values is StringValues strings )
                {
                    foreach ( var @string in strings )
                    {
                        list.Add( field.ConvertValue( @string ) );
                    }
                }
                else if ( values is IList repeated )
                {
                    foreach ( var value in repeated )
                    {
                        var item = field.Accessor.Descriptor.FieldType == FieldType.Message
                                   ? value
                                   : field.ConvertValue( value );

                        list.Add( item );
                    }
                }
                else
                {
                    list.Add( field.ConvertValue( values ) );
                }
            }
            else
            {
                if ( values is StringValues strings )
                {
                    if ( strings.Count == 1 )
                    {
                        field.Accessor.SetValue( message, field.ConvertValue( strings[0] ) );
                    }
                    else
                    {
                        throw new InvalidOperationException( SR.NonRepeatingMultipleValues );
                    }
                }
                else if ( values is IMessage nestedMessage )
                {
                    if ( nestedMessage.Descriptor.IsWrapperType )
                    {
                        const int WrapperValueFieldNumber = Int32Value.ValueFieldNumber;

                        var wrappedValue = nestedMessage.Descriptor.Fields[WrapperValueFieldNumber].Accessor.GetValue( nestedMessage );
                        field.Accessor.SetValue( message, wrappedValue );
                    }
                    else
                    {
                        field.Accessor.SetValue( message, nestedMessage );
                    }
                }
                else
                {
                    field.Accessor.SetValue( message, field.ConvertValue( values ) );
                }
            }
        }
    }
}