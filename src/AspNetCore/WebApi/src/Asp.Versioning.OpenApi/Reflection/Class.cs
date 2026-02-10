// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OpenApi.Reflection;

using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Linq.Expressions;
using static System.Linq.Expressions.Expression;

// HACK: all of these types are internal in Microsoft.AspNetCore.OpenApi
// REF: https://github.com/dotnet/aspnetcore/tree/main/src/OpenApi/src
internal static class Class
{
    public static class OpenApiDocumentService
    {
        private static readonly Func<IServiceProvider, string, object> factory = NewFactory();

        public static object New( IServiceProvider serviceProvider, string documentName ) => factory( serviceProvider, documentName );

        private static Func<IServiceProvider, string, object> NewFactory()
        {
            var constructor = Type.OpenApiDocumentService.GetConstructors().Single();
            var serviceProvider = Parameter( typeof( IServiceProvider ), "serviceProvider" );
            var documentName = Parameter( typeof( string ), "documentName" );
            var getService = typeof( IServiceProvider ).GetMethod( nameof( IServiceProvider.GetService ), [typeof( System.Type )] )!;
            var getRequiredService = typeof( ServiceProviderServiceExtensions ).GetMethod(
                nameof( ServiceProviderServiceExtensions.GetRequiredService ),
                [typeof( IServiceProvider ), typeof( System.Type )] )!;
            var apiDescriptionGroupCollectionProvider = typeof( IApiDescriptionGroupCollectionProvider );
            var hostEnvironment = typeof( IHostEnvironment );
            var optionsMonitor = typeof( IOptionsMonitor<OpenApiOptions> );
            var server = typeof( IServer );
            var body = Expression.New(
                constructor,
                documentName,
                Convert( Call( getRequiredService, serviceProvider, Constant( apiDescriptionGroupCollectionProvider ) ), apiDescriptionGroupCollectionProvider ),
                Convert( Call( getRequiredService, serviceProvider, Constant( hostEnvironment ) ), hostEnvironment ),
                Convert( Call( getRequiredService, serviceProvider, Constant( optionsMonitor ) ), optionsMonitor ),
                serviceProvider,
                Convert( Call( getRequiredService, serviceProvider, Constant( server ) ), server ) );
            var lambda = Lambda<Func<IServiceProvider, string, object>>( body, serviceProvider, documentName );

            return lambda.Compile();
        }
    }

    public static class OpenApiSchemaService
    {
        private static readonly Func<IServiceProvider, string, object> factory = NewFactory();

        public static object New( IServiceProvider serviceProvider, string documentName ) => factory( serviceProvider, documentName );

        private static Func<IServiceProvider, string, object> NewFactory()
        {
            var constructor = Type.OpenApiSchemaService.GetConstructors().Single();
            var serviceProvider = Parameter( typeof( IServiceProvider ), "serviceProvider" );
            var documentName = Parameter( typeof( string ), "documentName" );
            var getService = typeof( IServiceProvider ).GetMethod( nameof( IServiceProvider.GetService ), [typeof( System.Type )] )!;
            var getRequiredService = typeof( ServiceProviderServiceExtensions ).GetMethod(
                nameof( ServiceProviderServiceExtensions.GetRequiredService ),
                [typeof( IServiceProvider ), typeof( System.Type )] )!;
            var jsonOptions = typeof( IOptions<JsonOptions> );
            var optionsMonitor = typeof( IOptionsMonitor<OpenApiOptions> );
            var body = Expression.New(
                constructor,
                documentName,
                Convert( Call( getRequiredService, serviceProvider, Constant( jsonOptions ) ), jsonOptions ),
                Convert( Call( getRequiredService, serviceProvider, Constant( optionsMonitor ) ), optionsMonitor ) );
            var lambda = Lambda<Func<IServiceProvider, string, object>>( body, serviceProvider, documentName );

            return lambda.Compile();
        }
    }

    public static class OpenApiDocumentProvider
    {
        private static readonly Func<IServiceProvider, object> factory = NewFactory();

        public static object New( IServiceProvider serviceProvider ) => factory( serviceProvider );

        private static Func<IServiceProvider, object> NewFactory()
        {
            var constructor = Type.OpenApiDocumentProvider.GetConstructors().Single();
            var serviceProvider = Parameter( typeof( IServiceProvider ), "serviceProvider" );
            var getService = typeof( IServiceProvider ).GetMethod( nameof( IServiceProvider.GetService ), [typeof( System.Type )] )!;
            var getRequiredService = typeof( ServiceProviderServiceExtensions ).GetMethod(
                nameof( ServiceProviderServiceExtensions.GetRequiredService ),
                [typeof( IServiceProvider ), typeof( System.Type )] )!;
            var body = Expression.New( constructor, serviceProvider );
            var lambda = Lambda<Func<IServiceProvider, object>>( body, serviceProvider );

            return lambda.Compile();
        }
    }

    public static class NamedService
    {
        private static readonly Func<string, object> factory = NewFactory();

        public static object New( string name ) => factory( name );

        private static Func<string, object> NewFactory()
        {
            var constructor = Type.NamedService.GetConstructors().Single();
            var name = Parameter( typeof( string ), "name" );
            var body = Expression.New( constructor, name );
            var lambda = Lambda<Func<string, object>>( body, name );

            return lambda.Compile();
        }
    }
}