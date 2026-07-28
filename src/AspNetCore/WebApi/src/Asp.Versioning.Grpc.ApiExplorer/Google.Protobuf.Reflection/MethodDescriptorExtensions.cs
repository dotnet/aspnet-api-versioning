// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace Google.Protobuf.Reflection;

using Asp.Versioning;
using Asp.Versioning.Grpc;
using Google.Api;
using System.Text;
using static System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes;
using static System.Globalization.CultureInfo;
using Type = System.Type;

internal static class MethodDescriptorExtensions
{
    private static readonly CompositeFormat NestedBodyField = CompositeFormat.Parse( SR.NestedBodyField );
    private static readonly CompositeFormat NestedResponseBodyField = CompositeFormat.Parse( SR.NestedResponseBodyField );
    private static readonly CompositeFormat MissingBodyField = CompositeFormat.Parse( SR.MissingBodyField );
    private static readonly CompositeFormat MissingResponseBodyField = CompositeFormat.Parse( SR.MissingResponseBodyField );

    // transcoding assumes that the app is referencing google.api.commonprotos and httprule is from that assembly;
    // however, it's possible the app has compiled http.proto with grpc.tools, so the extension value is httprule from
    // a different assembly. this custom extension uses the httprule field number but has a return type of object.
    // the method always returns the extension value, and the calling code can convert it to the expected type.
    // see https://github.com/protocolbuffers/protobuf/issues/9626 for more details.
    private static readonly Extension<MethodOptions, object> UntypedHttpExtension =
        new( AnnotationsExtensions.Http.FieldNumber, codec: null );

    extension( MethodDescriptor methodDescriptor )
    {
        public bool TryGetHttpRule( [NotNullWhen( true )] out HttpRule? httpRule )
        {
            var options = methodDescriptor.GetOptions();
            var extension = options?.GetExtension( UntypedHttpExtension );

            // the untyped extension always returns the extension value. if the type is already the expected httprule
            // then use it directly. a different message indicates a custom httprule was used. convert the message to
            // bytes and reparse it to the known httprule type.
            httpRule = extension switch
            {
                HttpRule rule => rule,
                IMessage message => HttpRule.Parser.ParseFrom( message.ToByteArray() ),
                _ => null,
            };

            return httpRule != null;
        }

        public BodyDescriptorInfo? BodyDescriptor( string body, [DynamicallyAccessedMembers( PublicMethods )] Type serviceType )
        {
            if ( string.IsNullOrEmpty( body ) )
            {
                return default;
            }

            if ( string.Equals( body, "*", StringComparison.Ordinal ) )
            {
                var requestParameter = serviceType.GetMethod( methodDescriptor.Name ) is { } methodInfo
                    ? methodInfo.GetParameters().SingleOrDefault( p => p.Name == "request" )
                    : default;

                return new()
                {
                    Descriptor = methodDescriptor.InputType,
                    FieldDescriptor = default,
                    IsDescriptorRepeated = false,
                    ParameterInfo = requestParameter,
                    PropertyInfo = default,
                };
            }

            if ( body.Contains( '.', StringComparison.Ordinal ) )
            {
                throw new InvalidOperationException( string.Format( InvariantCulture, NestedBodyField, body ) );
            }

            if ( methodDescriptor.InputType.FindFieldByName( body ) is not { } bodyDescriptor )
            {
                throw new InvalidOperationException( string.Format( InvariantCulture, MissingBodyField, body, methodDescriptor.InputType.Name ) );
            }

            var propertyName = PascalCase.Format( bodyDescriptor.Name );
            var propertyInfo = bodyDescriptor.ContainingType.ClrType.GetProperty( propertyName );

            if ( bodyDescriptor.IsRepeated )
            {
                return new()
                {
                    Descriptor = bodyDescriptor.ContainingType,
                    FieldDescriptor = bodyDescriptor,
                    IsDescriptorRepeated = true,
                    ParameterInfo = default,
                    PropertyInfo = propertyInfo,
                };
            }

            return new()
            {
                Descriptor = bodyDescriptor.MessageType,
                FieldDescriptor = bodyDescriptor,
                IsDescriptorRepeated = false,
                ParameterInfo = default,
                PropertyInfo = propertyInfo,
            };
        }

        public FieldDescriptor? ResponseBodyDescriptor( string responseBody )
        {
            if ( string.IsNullOrEmpty( responseBody ) )
            {
                return default;
            }

            if ( responseBody.Contains( '.', StringComparison.Ordinal ) )
            {
                throw new InvalidOperationException( string.Format( InvariantCulture, NestedResponseBodyField, responseBody ) );
            }

            if ( methodDescriptor.OutputType.FindFieldByName( responseBody ) is not { } responseBodyDescriptor )
            {
                throw new InvalidOperationException( string.Format( InvariantCulture, MissingResponseBodyField, responseBody, methodDescriptor.OutputType.Name ) );
            }

            return responseBodyDescriptor;
        }

        public Dictionary<string, FieldDescriptor> QueryParameterDescriptors(
            Dictionary<string, RouteParameter> routeParameters,
            MessageDescriptor? bodyDescriptor,
            FieldDescriptor? bodyFieldDescriptor )
        {
            var queryParameters = new Dictionary<string, FieldDescriptor>();
            var existingParameters = new HashSet<FieldDescriptor>();

            foreach ( var routeParameter in routeParameters )
            {
                // each route field descriptors collection contains all the descriptors in the path. we only care about
                // the final place the route value is set and so add only the last descriptor to the existing
                // parameters collection
                existingParameters.Add( routeParameter.Value.DescriptorsPath.Last() );
            }

            if ( bodyDescriptor != null )
            {
                if ( bodyFieldDescriptor == null )
                {
                    // body with wildcard; all parameters are in the body so no query parameters
                    return queryParameters;
                }
                else
                {
                    // body with field name
                    existingParameters.Add( bodyFieldDescriptor );
                }
            }

            VisitMessages( queryParameters, existingParameters, methodDescriptor.InputType, [] );

            return queryParameters;
        }

        private static void VisitMessages(
            Dictionary<string, FieldDescriptor> queryParameters,
            HashSet<FieldDescriptor> existingParameters,
            MessageDescriptor messageDescriptor,
            List<FieldDescriptor> path )
        {
            var messageFields = messageDescriptor.Fields.InFieldNumberOrder();

            foreach ( var fieldDescriptor in messageFields )
            {
                // if a field is set via route parameter or body then don't add query parameter
                if ( existingParameters.Contains( fieldDescriptor ) )
                {
                    continue;
                }

                path.Add( fieldDescriptor );

                switch ( fieldDescriptor.FieldType )
                {
                    case FieldType.Double:
                    case FieldType.Float:
                    case FieldType.Int64:
                    case FieldType.UInt64:
                    case FieldType.Int32:
                    case FieldType.Fixed64:
                    case FieldType.Fixed32:
                    case FieldType.Bool:
                    case FieldType.String:
                    case FieldType.Bytes:
                    case FieldType.UInt32:
                    case FieldType.SFixed32:
                    case FieldType.SFixed64:
                    case FieldType.SInt32:
                    case FieldType.SInt64:
                    case FieldType.Enum:
                        {
                            var fullPath = string.Join( ".", path.Select( d => d.JsonName ) );
                            queryParameters[fullPath] = fieldDescriptor;
                        }

                        break;
                    case FieldType.Group:
                    case FieldType.Message:
                    default:
                        // complex repeated fields aren't valid query parameters
                        if ( fieldDescriptor.MessageType.IsCustomType )
                        {
                            var fullPath = string.Join( ".", path.Select( d => d.JsonName ) );
                            queryParameters[fullPath] = fieldDescriptor;
                        }
                        else if ( !fieldDescriptor.IsRepeated )
                        {
                            VisitMessages( queryParameters, existingParameters, fieldDescriptor.MessageType, path );
                        }

                        break;
                }

                path.RemoveAt( path.Count - 1 );
            }
        }
    }
}