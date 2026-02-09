// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace Microsoft.AspNetCore.Builder;

using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Asp.Versioning.OpenApi;
using Asp.Versioning.OpenApi.Internal;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Provides extension methods for <see cref="IEndpointConventionBuilder"/>.
/// </summary>
[CLSCompliant( false )]
public static class IEndpointConventionBuilderExtensions
{
    extension( IEndpointConventionBuilder builder )
    {
        /// <summary>
        /// Enables generating one OpenAPI document per APi Version for the associated endpoint builder.
        /// </summary>
        /// <remarks>
        /// This method is only intended to apply API Versioning conventions the OpenAPI endpoint. Applying this
        /// method to other endpoints may have unintended effects.
        /// </remarks>
        /// <returns>The original <see cref="IEndpointConventionBuilder">endpoint convention builder</see>.</returns>
        public IEndpointConventionBuilder WithDocumentPerVersion()
        {
            builder.Finally( new WithoutRecursion().Run );
            return builder;
        }
    }

    // This is a workaround to prevent infinite recursion when applying API versioning conventions to the OpenAPI
    // endpoint, which occurs because IApiVersionDescriptionProviderFactory.Create calls back into the endpoint's
    // RequestDelegate to resolve the IApiVersionDescriptionProvider, which causes the conventions to be applied again.
    private sealed class WithoutRecursion
    {
        private bool recursed;

        public void Run( EndpointBuilder builder )
        {
            if ( recursed )
            {
                return;
            }

            recursed = true;
            ApplyApiVersioning( builder );
            recursed = false;
        }
    }

    private static void ApplyApiVersioning( EndpointBuilder builder )
    {
        if ( builder.RequestDelegate is { } action )
        {
#pragma warning disable CA2000 // Dispose objects before losing scope
            var requestServices = NewRequestServices( builder.ApplicationServices );
#pragma warning restore CA2000 // Dispose objects before losing scope

            builder.RequestDelegate = context =>
            {
                context.RequestServices = requestServices;
                return action( context );
            };
        }
    }

    [UnconditionalSuppressMessage( "ILLink", "IL3050" )]
    private static KeyedServiceContainer NewRequestServices( IServiceProvider services )
    {
        var configure = services.GetRequiredKeyedService<Action<ApiVersionDescription, OpenApiOptions>>( typeof( ApiVersion ) );
        var factory = services.GetRequiredService<IApiVersionDescriptionProviderFactory>();
        var sources = services.GetRequiredService<ConfigureOpenApiOptions>().DataSources;
        var keyedServices = new KeyedServiceContainer( services );
        var names = new List<string>();
        IApiVersionDescriptionProvider provider;

        if ( sources.Count == 0 )
        {
            provider = factory.Create();
        }
        else
        {
            using var source = new CompositeEndpointDataSource( sources );
            provider = factory.Create( source );
        }

        foreach ( var description in provider.ApiVersionDescriptions )
        {
            names.Add( description.GroupName );
            keyedServices.Add( Type.OpenApiSchemaService, description.GroupName, Class.OpenApiSchemaService.New );
            keyedServices.Add( Type.OpenApiDocumentService, description.GroupName, Class.OpenApiDocumentService.New );
            keyedServices.Add(
                typeof( IOpenApiDocumentProvider ),
                description.GroupName,
                ( sp, k ) => sp.GetRequiredKeyedService( Type.OpenApiDocumentService, k ) );
        }

        if ( names.Count > 0 )
        {
            var array = Array.CreateInstance( Type.NamedService, names.Count );

            for ( var i = 0; i < names.Count; i++ )
            {
                array.SetValue( Class.NamedService.New( names[i] ), i );
            }

            keyedServices.Add( Type.IDocumentProvider, Class.OpenApiDocumentProvider.New );
            keyedServices.Add( Type.IEnumerableOfNamedService, array );
        }

        return keyedServices;
    }
}