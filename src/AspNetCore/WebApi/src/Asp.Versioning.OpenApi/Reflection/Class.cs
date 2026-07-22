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
using System.Runtime.CompilerServices;
using static System.Linq.Expressions.Expression;
using static System.Runtime.CompilerServices.UnsafeAccessorKind;

// HACK: all of these types are internal in Microsoft.AspNetCore.OpenApi
// REF: https://github.com/dotnet/aspnetcore/tree/main/src/OpenApi/src
internal static class Class
{
    public static class OpenApiDocumentService
    {
        public static object New( IServiceProvider serviceProvider, string documentName )
        {
            var apiDescriptionGroupCollectionProvider = serviceProvider.GetRequiredService<IApiDescriptionGroupCollectionProvider>();
            var hostEnvironment = serviceProvider.GetRequiredService<IHostEnvironment>();
            var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<OpenApiOptions>>();
            var server = serviceProvider.GetRequiredService<IServer>();

            return OpenApiDocumentServiceCtor(
                documentName,
                apiDescriptionGroupCollectionProvider,
                hostEnvironment,
                optionsMonitor,
                serviceProvider,
                server );
        }

        [UnsafeAccessor( Constructor )]
        [return: UnsafeAccessorType( Type.Name.OpenApiDocumentService )]
        private static extern object OpenApiDocumentServiceCtor(
            string documentName,
            IApiDescriptionGroupCollectionProvider apiDescriptionGroupCollectionProvider,
            IHostEnvironment hostEnvironment,
            IOptionsMonitor<OpenApiOptions> optionsMonitor,
            IServiceProvider serviceProvider,
            IServer server );
    }

    public static class OpenApiSchemaService
    {
        public static object New( IServiceProvider serviceProvider, string documentName )
        {
            var jsonOptions = serviceProvider.GetRequiredService<IOptions<JsonOptions>>();
            var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<OpenApiOptions>>();

            return OpenApiSchemaServiceCtor( documentName, jsonOptions, optionsMonitor );
        }

        [UnsafeAccessor( Constructor )]
        [return: UnsafeAccessorType( Type.Name.OpenApiSchemaService )]
        private static extern object OpenApiSchemaServiceCtor(
            string documentName,
            IOptions<JsonOptions> jsonOptions,
            IOptionsMonitor<OpenApiOptions> optionsMonitor );
    }

    public static class OpenApiDocumentProvider
    {
        public static object New( IServiceProvider serviceProvider ) => OpenApiDocumentProviderCtor( serviceProvider );

        [UnsafeAccessor( Constructor )]
        [return: UnsafeAccessorType( Type.Name.OpenApiDocumentProvider )]
        private static extern object OpenApiDocumentProviderCtor( IServiceProvider serviceProvider );
    }

    // this class cannot use UnsafeAccessor or UnsafeAccessorType because it is generic and neither the class nor the
    // type parameter is known at compile time
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