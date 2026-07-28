// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable CA1812

namespace Asp.Versioning.ApiExplorer;

using Asp.Versioning;
using Asp.Versioning.Grpc;
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
    IOptions<GrpcApiExplorerOptions> options ) : IApiDescriptionProvider
{
    private readonly Lazy<IRouteConstraint?> apiVersionRouteConstraint = new( NewRouteConstraint );

    // REF: https://github.com/dotnet/aspnetcore/blob/main/src/Mvc/Mvc.ApiExplorer/src/DefaultApiDescriptionProvider.cs
    public int Order => -900;

    [UnconditionalSuppressMessage( "IL3050", "IL3050", Justification = "Required gRPC types are never trimmed, but dynamically created and closed generic types might be" )]
    public void OnProvidersExecuting( ApiDescriptionProviderContext context )
    {
        var endpoints = source.Endpoints;

        foreach ( var endpoint in endpoints )
        {
            if ( endpoint is not RouteEndpoint routeEndpoint
                 || endpoint.Metadata.GetMetadata<GrpcJsonTranscodingMetadata>() is not { } metadata )
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

    public void OnProvidersExecuted( ApiDescriptionProviderContext context ) { }

    // ApiVersionRouteConstraint is critical to the versioned API Explorer, but gRPC doesn't use or even directly
    // reference API Versioning and can technically be used with out it. if the versioned API Explorer is present, then
    // this will never be trimmed. we don't need to do anything with this except create a single instance. failure is
    // acceptable and, in some cases, expected.
    private static IRouteConstraint? NewRouteConstraint()
    {
        const string TypeName = "Asp.Versioning.Routing.ApiVersionRouteConstraint, Asp.Versioning.Http";

        if ( Type.GetType( TypeName, throwOnError: false ) is { } type )
        {
            return Activator.CreateInstance( type ) as IRouteConstraint;
        }

        return default;
    }

    // the ApiVersion type is modeled as a first-class data type, but gRPC doesn't have a representation for it. when
    // we identify a parameter that represents an API version, explicitly set the expected data type and constraints
    // the versioned API Explorer expects.
    private (GrpcModelMetadata ModelMetadata, ApiParameterRouteInfo? RouteInfo) NewMetadataAndRouteInfo(
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

            if ( routeParameter && apiVersionRouteConstraint.Value is { } constraint )
            {
                routeInfo = new() { Constraints = [constraint] };
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
        var responseType = descriptor.ResponseBodyDescriptor( http.ResponseBody )?.ClrType ?? descriptor.OutputType.ClrType;
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
                    ModelMetadata = new GrpcModelMetadata( ModelMetadataIdentity.ForType( responseType ) ),
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
    private void AddRouteParameters(
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
            ModelMetadata = new GrpcModelMetadata( identity ),
            Source = BindingSource.Body,
            ParameterDescriptor = body.ParameterInfo is { } parameterInfo
            ? new() { ParameterInfo = parameterInfo }
            : default( ControllerParameterDescriptor )!,
        } );
    }

    [RequiresDynamicCode( "Might not be available at runtime" )]
    private void AddQueryParameters(
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