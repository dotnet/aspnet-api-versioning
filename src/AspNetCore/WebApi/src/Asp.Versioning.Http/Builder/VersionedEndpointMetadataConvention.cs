// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Builder;

using Asp.Versioning.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Globalization;
using static Asp.Versioning.ApiVersionParameterLocation;

internal sealed class VersionedEndpointMetadataConvention
{
    private readonly IEndpointMetadataBuilder metadataBuilder;

    public VersionedEndpointMetadataConvention( IEndpointMetadataBuilder metadataBuilder ) => this.metadataBuilder = metadataBuilder;

    public static implicit operator Action<EndpointBuilder>( VersionedEndpointMetadataConvention convention ) => convention.Apply;

    private static RequestDelegate EnsureRequestDelegate( RequestDelegate? current, RequestDelegate? original ) =>
        ( current ?? original ) ??
        throw new InvalidOperationException(
            string.Format(
                CultureInfo.CurrentCulture,
                SR.UnsetRequestDelegate,
                nameof( RequestDelegate ),
                nameof( RouteEndpoint ) ) );

    private void Apply( EndpointBuilder endpointBuilder )
    {
        var requestDelegate = default( RequestDelegate );
        var metadata = metadataBuilder.Build();
        var services = metadataBuilder.ServiceProvider;
        var source = services.GetRequiredService<IApiVersionParameterSource>();
        var options = services.GetRequiredService<IOptions<ApiVersioningOptions>>().Value;

        endpointBuilder.Metadata.Add( metadata );

        if ( metadataBuilder.ReportApiVersions || options.ReportApiVersions )
        {
            requestDelegate = EnsureRequestDelegate( requestDelegate, endpointBuilder.RequestDelegate );
            requestDelegate = new ReportApiVersionsDecorator( requestDelegate, metadata );
            endpointBuilder.RequestDelegate = requestDelegate;
        }

        if ( source.VersionsByMediaType() )
        {
            var parameterName = source.GetParameterName( MediaTypeParameter );
            requestDelegate = EnsureRequestDelegate( requestDelegate, endpointBuilder.RequestDelegate );
            requestDelegate = new ContentTypeApiVersionDecorator( requestDelegate, parameterName );
            endpointBuilder.RequestDelegate = requestDelegate;
        }
    }
}