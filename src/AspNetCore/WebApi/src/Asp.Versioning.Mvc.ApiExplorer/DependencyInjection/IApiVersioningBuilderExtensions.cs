// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Microsoft.Extensions.DependencyInjection;

using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
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
        if ( builder == null )
        {
            throw new ArgumentNullException( nameof( builder ) );
        }

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
        if ( builder == null )
        {
            throw new ArgumentNullException( nameof( builder ) );
        }

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
        var options = serviceProvider.GetRequiredService<IOptions<ApiExplorerOptions>>();
        var mightUseCustomGroups = options.Value.FormatGroupName is not null;

        return new ApiVersionDescriptionProviderFactory( serviceProvider, mightUseCustomGroups ? NewGroupedProvider : NewDefaultProvider );

        static IApiVersionDescriptionProvider NewDefaultProvider(
            EndpointDataSource endpointDataSource,
            IActionDescriptorCollectionProvider actionDescriptorCollectionProvider,
            ISunsetPolicyManager sunsetPolicyManager,
            IOptions<ApiExplorerOptions> apiExplorerOptions ) =>
            new DefaultApiVersionDescriptionProvider( endpointDataSource, actionDescriptorCollectionProvider, sunsetPolicyManager, apiExplorerOptions );

        static IApiVersionDescriptionProvider NewGroupedProvider(
            EndpointDataSource endpointDataSource,
            IActionDescriptorCollectionProvider actionDescriptorCollectionProvider,
            ISunsetPolicyManager sunsetPolicyManager,
            IOptions<ApiExplorerOptions> apiExplorerOptions ) =>
            new GroupedApiVersionDescriptionProvider( endpointDataSource, actionDescriptorCollectionProvider, sunsetPolicyManager, apiExplorerOptions );
    }

    private static IApiVersionDescriptionProvider ResolveApiVersionDescriptionProvider( IServiceProvider serviceProvider )
    {
        var endpointDataSource = serviceProvider.GetRequiredService<EndpointDataSource>();
        var actionDescriptorCollectionProvider = serviceProvider.GetRequiredService<IActionDescriptorCollectionProvider>();
        var sunsetPolicyManager = serviceProvider.GetRequiredService<ISunsetPolicyManager>();
        var options = serviceProvider.GetRequiredService<IOptions<ApiExplorerOptions>>();
        var mightUseCustomGroups = options.Value.FormatGroupName is not null;

        if ( mightUseCustomGroups )
        {
            return new GroupedApiVersionDescriptionProvider(
                endpointDataSource,
                actionDescriptorCollectionProvider,
                sunsetPolicyManager,
                options );
        }

        return new DefaultApiVersionDescriptionProvider(
            endpointDataSource,
            actionDescriptorCollectionProvider,
            sunsetPolicyManager,
            options );
    }
}