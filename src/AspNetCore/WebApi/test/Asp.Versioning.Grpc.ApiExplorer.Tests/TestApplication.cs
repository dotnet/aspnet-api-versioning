// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

// the API descriptions are only produced from a live EndpointDataSource, so the endpoints have to be
// materialized by an application that has started
internal static class TestApplication
{
    public static async Task<IReadOnlyList<ApiDescription>> DescribeApisAsync(
        Action<IServiceCollection> configureServices = default )
    {
        var options = new WebApplicationOptions() { ContentRootPath = AppContext.BaseDirectory };
        var builder = WebApplication.CreateEmptyBuilder( options );

        builder.WebHost.UseTestServer();
        builder.Services.AddRouting();
        builder.Services.AddApiVersioning().AddGrpc().AddGrpcApiExplorer();
        configureServices?.Invoke( builder.Services );

        var app = builder.Build();

        app.UseRouting();
        app.MapGrpcService<TestOrdersService>();

        await app.StartAsync().ConfigureAwait( false );

        try
        {
            var provider = app.Services.GetRequiredService<IApiDescriptionGroupCollectionProvider>();
            return [.. provider.ApiDescriptionGroups.Items.SelectMany( group => group.Items )];
        }
        finally
        {
            await app.StopAsync().ConfigureAwait( false );
            await app.DisposeAsync().ConfigureAwait( false );
        }
    }

    public static async Task<ApiDescription> DescribeApiAsync(
        string httpMethod,
        string relativePath,
        Action<IServiceCollection> configureServices = default )
    {
        var descriptions = await DescribeApisAsync( configureServices ).ConfigureAwait( false );
        var comparer = StringComparer.OrdinalIgnoreCase;

        return descriptions.Single(
            description => comparer.Equals( description.HttpMethod, httpMethod )
                           && comparer.Equals( description.RelativePath, relativePath ) );
    }
}