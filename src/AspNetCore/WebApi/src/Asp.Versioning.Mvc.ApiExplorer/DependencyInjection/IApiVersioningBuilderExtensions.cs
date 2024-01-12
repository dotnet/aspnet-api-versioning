// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Microsoft.Extensions.DependencyInjection;

using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using static ServiceDescriptor;

/// <summary>
/// Provides ASP.NET Core API Explorer specific extension methods for <see cref="IApiVersioningBuilder"/>.
/// </summary>
[CLSCompliant( false )]
public static class IApiVersioningBuilderExtensions
{
    /// <summary>
    /// Adds the API versioning extensions for the API Explorer.
    /// </summary>
    /// <param name="builder">The extended <see cref="IApiVersioningBuilder">API versioning builder</see>.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static IApiVersioningBuilder AddApiExplorer( this IApiVersioningBuilder builder )
    {
        ArgumentNullException.ThrowIfNull( builder );
        AddApiExplorerServices( builder );
        return builder;
    }

    /// <summary>
    /// Adds the API versioning extensions for the API Explorer.
    /// </summary>
    /// <param name="builder">The extended <see cref="IApiVersioningBuilder">API versioning builder</see>.</param>
    /// <param name="setupAction">An <see cref="Action{T}">action</see> used to configure the provided options.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static IApiVersioningBuilder AddApiExplorer( this IApiVersioningBuilder builder, Action<ApiExplorerOptions> setupAction )
    {
        ArgumentNullException.ThrowIfNull( builder );
        AddApiExplorerServices( builder );
        builder.Services.Configure( setupAction );
        return builder;
    }

    private static void AddApiExplorerServices( IApiVersioningBuilder builder )
    {
        builder.AddMvc();

        var services = builder.Services;

        services.AddMvcCore().AddApiExplorer();
        services.TryAddSingleton<IOptionsFactory<ApiExplorerOptions>, ApiExplorerOptionsFactory<ApiExplorerOptions>>();
        services.TryAddTransient( ResolveApiVersionDescriptionProviderFactory );
        services.TryAddSingleton( ResolveApiVersionDescriptionProvider );

        // use internal constructor until ASP.NET Core fixes their bug
        // BUG: https://github.com/dotnet/aspnetcore/issues/41773
        services.TryAddEnumerable(
            Transient<IApiDescriptionProvider, VersionedApiDescriptionProvider>(
                sp => new(
                    sp.GetRequiredService<ISunsetPolicyManager>(),
                    sp.GetRequiredService<IModelMetadataProvider>(),
                    sp.GetRequiredService<IInlineConstraintResolver>(),
                    sp.GetRequiredService<IOptions<ApiExplorerOptions>>() ) ) );
    }

    private static IApiVersionDescriptionProviderFactory ResolveApiVersionDescriptionProviderFactory( IServiceProvider serviceProvider )
    {
        var sunsetPolicyManager = serviceProvider.GetRequiredService<ISunsetPolicyManager>();
        var providers = serviceProvider.GetServices<IApiVersionMetadataCollationProvider>();
        var inspector = serviceProvider.GetRequiredService<IEndpointInspector>();
        var options = serviceProvider.GetRequiredService<IOptions<ApiExplorerOptions>>();

        return new ApiVersionDescriptionProviderFactory(
            NewDefaultProvider,
            sunsetPolicyManager,
            providers,
            inspector,
            options );

        static DefaultApiVersionDescriptionProvider NewDefaultProvider(
            IEnumerable<IApiVersionMetadataCollationProvider> providers,
            ISunsetPolicyManager sunsetPolicyManager,
            IOptions<ApiExplorerOptions> apiExplorerOptions ) =>
            new( providers, sunsetPolicyManager, apiExplorerOptions );
    }

    private static IApiVersionDescriptionProvider ResolveApiVersionDescriptionProvider( IServiceProvider serviceProvider )
    {
        var factory = serviceProvider.GetRequiredService<IApiVersionDescriptionProviderFactory>();
        var endpointDataSource = new EmptyEndpointDataSource();
        return factory.Create( endpointDataSource );
    }

    private sealed class EmptyEndpointDataSource : EndpointDataSource
    {
        public override IReadOnlyList<Endpoint> Endpoints => Array.Empty<Endpoint>();

        public override IChangeToken GetChangeToken() => new CancellationChangeToken( CancellationToken.None );

        public override IReadOnlyList<Endpoint> GetGroupedEndpoints( RouteGroupContext context ) => Array.Empty<Endpoint>();
    }
}