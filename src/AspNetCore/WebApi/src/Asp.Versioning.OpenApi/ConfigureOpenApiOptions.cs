// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable CA1812

namespace Asp.Versioning.OpenApi;

using Asp.Versioning.ApiExplorer;
using Asp.Versioning.OpenApi.Internal;
using Asp.Versioning.OpenApi.Transformers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

internal sealed class ConfigureOpenApiOptions(
    IHostEnvironment environment,
    IApiVersionDescriptionProviderFactory factory,
    [FromKeyedServices( typeof( ApiVersion ) )] Action<ApiVersionDescription, OpenApiOptions> configure )
    : IConfigureNamedOptions<OpenApiOptions>
{
    private readonly IApiVersionDescriptionProviderFactory factory = factory;
    private readonly Action<ApiVersionDescription, OpenApiOptions> configure = configure;

    internal ICollection<EndpointDataSource> DataSources { get; set; } = [];

    public void Configure( string? name, OpenApiOptions options )
    {
        var comparer = StringComparer.OrdinalIgnoreCase;
        IApiVersionDescriptionProvider provider;

        if ( DataSources.Count == 0 )
        {
            provider = factory.Create();
        }
        else
        {
            using var source = new CompositeEndpointDataSource( DataSources );
            provider = factory.Create( source );
        }

        var descriptions = provider.ApiVersionDescriptions;
        var xmlComments = new XmlCommentsTransformer( environment );

        for ( var i = 0; i < descriptions.Count; i++ )
        {
            var description = descriptions[i];

            if ( !comparer.Equals( name, description.GroupName ) )
            {
                continue;
            }

            var apiExplorer = new ApiExplorerTransformer(description);

            options.SetDocumentName( description.GroupName );
            options.AddDocumentTransformer( apiExplorer );
            options.AddSchemaTransformer( apiExplorer );
            options.AddOperationTransformer( apiExplorer );

            if ( !xmlComments.IsEmpty )
            {
                options.AddSchemaTransformer( xmlComments );
                options.AddOperationTransformer( xmlComments );
            }

            configure( description, options );
            break;
        }
    }

    public void Configure( OpenApiOptions options )
    {
        // intentionally empty; all options must be named
    }
}