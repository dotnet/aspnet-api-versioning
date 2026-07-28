// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace Google.Protobuf.Reflection;

using Asp.Versioning;
using Asp.Versioning.Grpc;
using Google.Protobuf.WellKnownTypes;
using System.Text;
using static System.Globalization.CultureInfo;

internal static class MessageDescriptorExtensions
{
    private static readonly CompositeFormat MissingFieldForRouteParam = CompositeFormat.Parse( SR.MissingFieldForRouteParam );
    private static readonly HashSet<string> WellKnownTypeNames =
    [
        "google/protobuf/any.proto",
        "google/protobuf/api.proto",
        "google/protobuf/duration.proto",
        "google/protobuf/empty.proto",
        "google/protobuf/wrappers.proto",
        "google/protobuf/timestamp.proto",
        "google/protobuf/field_mask.proto",
        "google/protobuf/source_context.proto",
        "google/protobuf/struct.proto",
        "google/protobuf/type.proto",
    ];

    extension( MessageDescriptor messageDescriptor )
    {
        public bool IsWellKnownType => messageDescriptor.File.Package == "google.protobuf"
                                       && WellKnownTypeNames.Contains( messageDescriptor.File.Name );

        // keep this in sync with GrpcDataContractResolver.TryCustomizeMessage
        public bool IsCustomType =>
            messageDescriptor.IsWrapperType
            || messageDescriptor.FullName == Timestamp.Descriptor.FullName
            || messageDescriptor.FullName == Duration.Descriptor.FullName
            || messageDescriptor.FullName == FieldMask.Descriptor.FullName
            || messageDescriptor.FullName == Struct.Descriptor.FullName
            || messageDescriptor.FullName == ListValue.Descriptor.FullName
            || messageDescriptor.FullName == Value.Descriptor.FullName
            || messageDescriptor.FullName == Any.Descriptor.FullName;

        public bool TryResolveDescriptors(
            IList<string> path,
            bool allowJsonName,
            [NotNullWhen( true )] out List<FieldDescriptor>? fieldDescriptors )
        {
            fieldDescriptors = default;
            var currentDescriptor = messageDescriptor;

            foreach ( var fieldName in path )
            {
                var field = default( FieldDescriptor );

                if ( currentDescriptor != null )
                {
                    field = allowJsonName
                        ? GetFieldByName( currentDescriptor, fieldName )
                        : currentDescriptor.FindFieldByName( fieldName );
                }

                if ( field == null )
                {
                    fieldDescriptors = null;
                    return false;
                }

                fieldDescriptors ??= [];
                fieldDescriptors.Add( field );

                currentDescriptor = field.FieldType == FieldType.Message ? field.MessageType : null;
            }

            return fieldDescriptors != null;
        }

        public Dictionary<string, RouteParameter> RouteParameterDescriptors( List<HttpRouteVariable> variables )
        {
            var parameterDescriptors = new Dictionary<string, RouteParameter>( StringComparer.Ordinal );

            foreach ( var variable in variables )
            {
                var path = variable.FieldPath;

                if ( !messageDescriptor.TryResolveDescriptors( path, allowJsonName: false, out var fieldDescriptors ) )
                {
                    var message = string.Format( InvariantCulture, MissingFieldForRouteParam, string.Join( ".", path ), messageDescriptor.Name );
                    throw new InvalidOperationException( message );
                }

                var fieldPath = string.Join( ".", fieldDescriptors.Select( d => d.Name ) );
                var jsonPath = string.Join( ".", fieldDescriptors.Select( d => d.JsonName ) );

                parameterDescriptors.Add( fieldPath, new( fieldDescriptors, variable, jsonPath ) );
            }

            return parameterDescriptors;
        }

        // search fields by field name and json name. both names can be referenced. the json name takes precedence. if
        // there are conflicts, then the last field with a name wins. this logic matches how properties are used in
        // the json serialization.
        private FieldDescriptor? GetFieldByName( string fieldName )
        {
            var fields = messageDescriptor.Fields.InFieldNumberOrder();
            var fieldDescriptor = default( FieldDescriptor );

            for ( var i = fields.Count - 1; i >= 0; i-- )
            {
                var field = fields[i];

                if ( field.JsonName == fieldName )
                {
                    return field;
                }

                if ( fieldDescriptor is null && field.Name == fieldName )
                {
                    fieldDescriptor = field;
                }
            }

            return fieldDescriptor;
        }
    }
}