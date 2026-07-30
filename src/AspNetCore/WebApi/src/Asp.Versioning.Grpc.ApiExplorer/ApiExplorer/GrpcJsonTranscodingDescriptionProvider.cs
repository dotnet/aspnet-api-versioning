// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable CA1812

namespace Asp.Versioning.ApiExplorer;

using Asp.Versioning;
using Asp.Versioning.Grpc;
using Asp.Versioning.Routing;
using global::Grpc.AspNetCore.Server;
using Google.Api;
using Google.Protobuf.Reflection;
using Microsoft.AspNetCore.Grpc.JsonTranscoding;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using System.Reflection;
using static System.Net.Mime.MediaTypeNames;

internal sealed class GrpcJsonTranscodingDescriptionProvider(
    EndpointDataSource source,
    FileDescriptorPool pool,
    IMemberFilter<FieldDescriptor> filter,
    IOptions<GrpcApiExplorerOptions> options ) : IApiDescriptionProvider
{
    private static readonly ApiVersionRouteConstraint ApiVersionRouteConstraint = new();

    // REF: https://github.com/dotnet/aspnetcore/blob/main/src/Mvc/Mvc.ApiExplorer/src/DefaultApiDescriptionProvider.cs
    public int Order => -900;

    [UnconditionalSuppressMessage( "IL3050", "IL3050", Justification = "Required gRPC types are never trimmed, but dynamically created and closed generic types might be" )]
    public void OnProvidersExecuting( ApiDescriptionProviderContext context )
    {
        var endpoints = source.Endpoints;

        foreach ( var endpoint in endpoints )
        {
            if ( endpoint is not RouteEndpoint routeEndpoint ||
                 endpoint.Metadata.GetMetadata<GrpcJsonTranscodingMetadata>() is not { } metadata )
            {
                continue;
            }

            var http = metadata.HttpRule;
            var descriptor = metadata.MethodDescriptor;

            if ( http.TryResolvePattern( out var pattern, out var method ) )
            {
                context.Results.Add( NewApiDescription( routeEndpoint, http, descriptor, pattern, method ) );
                pool.Add( metadata.MethodDescriptor.File );
            }
        }
    }

    // OnProvidersExecuting runs in ascending order, but OnProvidersExecuted runs in descending order. this provider is
    // ordered before the versioned API Explorer, which means it runs after the versioned API Explorer has expanded each
    // result into one API description per API version and assigned ApiDescription.ApiVersion. that expansion clones the
    // parameter and response descriptions, so the fields that don't apply to the version being described can be removed
    // here without affecting the other versions.
    public void OnProvidersExecuted( ApiDescriptionProviderContext context )
    {
        var results = context.Results;

        for ( var i = 0; i < results.Count; i++ )
        {
            var result = results[i];

            // the ApiVersion property is set by the versioned API Explorer, which is not referenced by design. read
            // the well-known property directly instead so gRPC continues to work without API versioning
            if ( result.GetProperty<ApiVersion>() is not { } apiVersion ||
                 GetTranscodingMetadata( result ) is not { } metadata )
            {
                continue;
            }

            RemoveExcludedParameters( result, metadata.MethodDescriptor.InputType, apiVersion );
            ApplyApiVersionToMessages( result, apiVersion );
        }
    }

    private static GrpcJsonTranscodingMetadata? GetTranscodingMetadata( ApiDescription apiDescription )
    {
        var endpointMetadata = apiDescription.ActionDescriptor.EndpointMetadata;

        if ( endpointMetadata is null )
        {
            return default;
        }

        for ( var i = 0; i < endpointMetadata.Count; i++ )
        {
            if ( endpointMetadata[i] is GrpcJsonTranscodingMetadata metadata )
            {
                return metadata;
            }
        }

        return default;
    }

    // route and query parameters are flattened field paths of the input message, so an excluded field anywhere along
    // the path excludes the parameter. a name that doesn't resolve to a field isn't ours - notably the API version
    // parameter added by the versioned API Explorer - and is always left alone
    private void RemoveExcludedParameters( ApiDescription apiDescription, MessageDescriptor input, ApiVersion apiVersion )
    {
        var parameters = apiDescription.ParameterDescriptions;

        for ( var i = parameters.Count - 1; i >= 0; i-- )
        {
            var parameter = parameters[i];

            if ( parameter.Source != BindingSource.Path && parameter.Source != BindingSource.Query )
            {
                continue;
            }

            var path = parameter.Name.Split( '.' );

            if ( !input.TryResolveDescriptors( path, allowJsonName: true, out var fields ) )
            {
                continue;
            }

            for ( var j = 0; j < fields.Count; j++ )
            {
                if ( !filter.IsVisible( fields[j], apiVersion ) )
                {
                    parameters.RemoveAt( i );
                    break;
                }
            }
        }
    }

    // the body and response types are message types whose members cannot be expressed as a subset by the CLR type
    // alone. the model metadata is rebound to the version being described so that the excluded members are dropped
    // from the reported properties of the message and of every message nested within it
    private void ApplyApiVersionToMessages( ApiDescription apiDescription, ApiVersion apiVersion )
    {
        var parameters = apiDescription.ParameterDescriptions;

        for ( var i = 0; i < parameters.Count; i++ )
        {
            var parameter = parameters[i];

            if ( parameter.Source == BindingSource.Body &&
                 parameter.ModelMetadata is GrpcModelMetadata metadata )
            {
                parameter.ModelMetadata = metadata.ForApiVersion( filter, apiVersion );
            }
        }

        var responseTypes = apiDescription.SupportedResponseTypes;

        for ( var i = 0; i < responseTypes.Count; i++ )
        {
            var responseType = responseTypes[i];

            if ( responseType.ModelMetadata is GrpcModelMetadata metadata )
            {
                responseType.ModelMetadata = metadata.ForApiVersion( filter, apiVersion );
            }
        }
    }

    // the ApiVersion type is modeled as a first-class data type, but gRPC doesn't have a representation for it. when
    // we identify a parameter that represents an API version, explicitly set the expected data type and constraints
    // the versioned API Explorer expects.
    private static (GrpcModelMetadata ModelMetadata, ApiParameterRouteInfo? RouteInfo) NewMetadataAndRouteInfo(
        string name,
        ModelMetadataIdentity identity,
        GrpcApiExplorerOptions options,
        bool routeParameter )
    {
        var metadata = new GrpcModelMetadata( identity );
        var routeInfo = default( ApiParameterRouteInfo );

        if ( StringComparer.Ordinal.Equals( name, options.RouteParameter.Name ) )
        {
            metadata.SetDataTypeName( "ApiVersion" );

            if ( routeParameter )
            {
                routeInfo = new() { Constraints = [ApiVersionRouteConstraint] };
            }
        }

        return (metadata, routeInfo);
    }

    [RequiresDynamicCode( "Might not be available at runtime" )]
    private ApiDescription NewApiDescription(
        RouteEndpoint endpoint,
        HttpRule http,
        MethodDescriptor descriptor,
        string pattern,
        string httpMethod )
    {
        var metadata = endpoint.Metadata.GetMetadata<GrpcMethodMetadata>()!;
        var routePattern = HttpRoutePattern.Parse( pattern, options.Value );
        var routeParameters = descriptor.InputType.RouteParameterDescriptors( routePattern.Variables );
        var responseBody = descriptor.ResponseBodyDescriptor( http.ResponseBody );
        var responseType = responseBody?.ClrType ?? descriptor.OutputType.ClrType;
        var responseMessage = responseBody is null
            ? descriptor.OutputType
            : responseBody.FieldType == FieldType.Message && !responseBody.IsMap ? responseBody.MessageType : default;
        var apiDescription = new ApiDescription
        {
            ActionDescriptor = new ControllerActionDescriptor()
            {
                ActionName = descriptor.Name,
                ControllerName = descriptor.Service.Name,
                ControllerTypeInfo = metadata.ServiceType.GetTypeInfo(),
                MethodInfo = metadata.ServiceType.GetTypeInfo().GetMethod( descriptor.Name )!,
                EndpointMetadata = [.. endpoint.Metadata],
                RouteValues = new Dictionary<string, string?>( StringComparer.OrdinalIgnoreCase )
                {
                    ["action"] = descriptor.Name,
                    ["controller"] = descriptor.Service.Name,
                },
            },
            GroupName = endpoint.Metadata.GetMetadata<ApiExplorerSettingsAttribute>()?.GroupName,
            HttpMethod = httpMethod,
            RelativePath = routePattern.BuildPath( routeParameters ),
            SupportedRequestFormats =
            {
                new() { MediaType = Application.Json },
            },
            SupportedResponseTypes =
            {
                new()
                {
                    ApiResponseFormats = { new() { MediaType = Application.Json } },
                    Type = responseType,
                    ModelMetadata = new GrpcModelMetadata( ModelMetadataIdentity.ForType( responseType ), responseMessage ),
                    StatusCode = 200,
                },
                new()
                {
                    ApiResponseFormats = { new() { MediaType = Application.Json } },
                    Type = typeof( Google.Rpc.Status ),
                    ModelMetadata = new GrpcModelMetadata( ModelMetadataIdentity.ForType( typeof( Google.Rpc.Status ) ) ),
                    IsDefaultResponse = true,
                },
            },
        };
        var bodyDescriptor = descriptor.BodyDescriptor( http.Body, metadata.ServiceType );
        var queryParameters = descriptor.QueryParameterDescriptors(
            routeParameters,
            bodyDescriptor?.Descriptor,
            bodyDescriptor?.FieldDescriptor );

        AddRouteParameters( apiDescription, routeParameters, options.Value );
        AddBodyParameter( apiDescription, bodyDescriptor );
        AddQueryParameters( apiDescription, queryParameters, options.Value );

        return apiDescription;
    }

    [RequiresDynamicCode( "Might not be available at runtime" )]
    private static void AddRouteParameters(
        ApiDescription apiDescription,
        Dictionary<string, RouteParameter> parameters,
        GrpcApiExplorerOptions options )
    {
        foreach ( var (key, parameter) in parameters )
        {
            var field = parameter.DescriptorsPath.Last();
            var name = PascalCase.Format( key );
            var parameterInfo = default( PropertyParameterInfo )!;
            ModelMetadataIdentity identity;

            if ( field.ContainingType.ClrType.GetProperty( name ) is { } propertyInfo )
            {
                identity = ModelMetadataIdentity.ForProperty( propertyInfo, field.ClrType, field.ContainingType.ClrType );
                parameterInfo = new( propertyInfo );
            }
            else
            {
                identity = ModelMetadataIdentity.ForType( field.ClrType );
            }

            var (metadata, routeInfo) = NewMetadataAndRouteInfo( key, identity, options, routeParameter: true );

            apiDescription.ParameterDescriptions.Add( new()
            {
                Name = parameter.JsonPath,
                Type = identity.ModelType,
                ModelMetadata = metadata,
                Source = BindingSource.Path,
                IsRequired = true,
                RouteInfo = routeInfo,
                ParameterDescriptor = new ControllerParameterDescriptor()
                {
                    Name = name,
                    ParameterType = identity.ModelType,
                    ParameterInfo = parameterInfo,
                },
            } );
        }
    }

    private static void AddBodyParameter( ApiDescription apiDescription, BodyDescriptorInfo? body )
    {
        if ( body is null )
        {
            return;
        }

        var identity = body.PropertyInfo is { } propertyInfo
            ? ModelMetadataIdentity.ForProperty( propertyInfo, propertyInfo.PropertyType, propertyInfo.DeclaringType! )
            : ModelMetadataIdentity.ForType( body.Descriptor.ClrType );

        apiDescription.ParameterDescriptions.Add( new()
        {
            Name = "Input",
            Type = identity.ModelType,
            ModelMetadata = new GrpcModelMetadata( identity, body.Descriptor ),
            Source = BindingSource.Body,
            ParameterDescriptor = body.ParameterInfo is { } parameterInfo
            ? new() { ParameterInfo = parameterInfo }
            : default( ControllerParameterDescriptor )!,
        } );
    }

    [RequiresDynamicCode( "Might not be available at runtime" )]
    private static void AddQueryParameters(
        ApiDescription apiDescription,
        Dictionary<string, FieldDescriptor> parameters,
        GrpcApiExplorerOptions options )
    {
        foreach ( var (name, field) in parameters )
        {
            var parameterInfo = default( PropertyParameterInfo )!;
            ModelMetadataIdentity identity;

            if ( field.ContainingType.ClrType.GetProperty( field.PropertyName ) is { } propertyInfo )
            {
                identity = ModelMetadataIdentity.ForProperty( propertyInfo, field.ClrType, field.ContainingType.ClrType );
                parameterInfo = new( propertyInfo );
            }
            else
            {
                identity = ModelMetadataIdentity.ForType( field.ClrType );
            }

            var (metadata, routeInfo) = NewMetadataAndRouteInfo( name, identity, options, routeParameter: false );

            apiDescription.ParameterDescriptions.Add( new()
            {
                Name = name,
                Type = identity.ModelType,
                ModelMetadata = metadata,
                Source = BindingSource.Query,
                IsRequired = field.IsRequired,
                RouteInfo = routeInfo,
                ParameterDescriptor = new ControllerParameterDescriptor()
                {
                    Name = field.PropertyName,
                    ParameterType = identity.ModelType,
                    ParameterInfo = parameterInfo,
                },
            } );
        }
    }
}