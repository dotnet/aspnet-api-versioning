// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Microsoft.Extensions.DependencyInjection;

using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Asp.Versioning.Conventions;
using Asp.Versioning.OData;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using static Microsoft.Extensions.DependencyInjection.ServiceDescriptor;

/// <summary>
/// Provides extension methods for the <see cref="IServiceCollection"/> interface.
/// </summary>
[CLSCompliant( false )]
public static class IApiVersioningBuilderExtensions
{
    /// <summary>
    /// Adds the API versioning extensions for the API Explorer with OData.
    /// </summary>
    /// <param name="builder">The extended <see cref="IApiVersioningBuilder">API versioning builder</see>.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static IApiVersioningBuilder AddODataApiExplorer( this IApiVersioningBuilder builder )
    {
        ArgumentNullException.ThrowIfNull( builder );
        AddApiExplorerServices( builder );
        return builder;
    }

    /// <summary>
    /// Adds the API versioning extensions for the API Explorer with OData.
    /// </summary>
    /// <param name="builder">The extended <see cref="IApiVersioningBuilder">API versioning builder</see>.</param>
    /// <param name="setupAction">An <see cref="Action{T}">action</see> used to configure the provided options.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static IApiVersioningBuilder AddODataApiExplorer( this IApiVersioningBuilder builder, Action<ODataApiExplorerOptions> setupAction )
    {
        ArgumentNullException.ThrowIfNull( builder );
        AddApiExplorerServices( builder );
        builder.Services.Configure( setupAction );
        return builder;
    }

    private static void AddApiExplorerServices( IApiVersioningBuilder builder )
    {
        var services = builder.Services;

        builder.AddApiExplorer();
        builder.Services.AddModelConfigurationsAsServices();
        services.TryAddSingleton<IModelTypeBuilder, DefaultModelTypeBuilder>();
        services.TryAddSingleton<IOptionsFactory<ODataApiExplorerOptions>, ODataApiExplorerOptionsFactory>();
        services.TryAddEnumerable( Transient<IApiDescriptionProvider, PartialODataDescriptionProvider>() );
        services.TryAddEnumerable( Transient<IApiDescriptionProvider, ODataApiDescriptionProvider>() );
        services.TryAddEnumerable( Transient<IModelConfiguration, ImplicitModelBoundSettingsConvention>() );
        services.Replace( Singleton<IOptionsFactory<ApiExplorerOptions>, ODataApiExplorerOptionsAdapter>() );
    }

#pragma warning disable IDE0079
#pragma warning disable CA1812

    private sealed class ODataApiExplorerOptionsAdapter( IOptionsFactory<ODataApiExplorerOptions> factory )
        : IOptionsFactory<ApiExplorerOptions>
    {
        public ApiExplorerOptions Create( string name ) => factory.Create( name );
    }
}