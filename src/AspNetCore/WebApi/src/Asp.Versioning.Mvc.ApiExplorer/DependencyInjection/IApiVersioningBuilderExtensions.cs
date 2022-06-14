// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Microsoft.Extensions.DependencyInjection;

using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
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

        AddApiExplorerServices( builder.Services );
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

        var services = builder.Services;
        AddApiExplorerServices( services );
        services.Configure( setupAction );
        return builder;
    }

    private static void AddApiExplorerServices( IServiceCollection services )
    {
        if ( services == null )
        {
            throw new ArgumentNullException( nameof( services ) );
        }

        services.AddMvcCore().AddApiExplorer();
        services.TryAddSingleton<IOptionsFactory<ApiExplorerOptions>, ApiExplorerOptionsFactory<ApiExplorerOptions>>();
        services.TryAddSingleton<IApiVersionDescriptionProvider, DefaultApiVersionDescriptionProvider>();

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
}