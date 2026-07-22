// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OpenApi.Reflection;

using static System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes;

// HACK: all of these types are internal in Microsoft.AspNetCore.OpenApi
// REF: https://github.com/dotnet/aspnetcore/tree/main/src/OpenApi/src
internal static class Type
{
    [DynamicallyAccessedMembers( PublicConstructors )]
    public static readonly System.Type IDocumentProvider = System.Type.GetType( Name.IDocumentProvider, throwOnError: true )!;

    [DynamicallyAccessedMembers( PublicConstructors )]
    public static readonly System.Type NamedService = System.Type.GetType( Name.NamedService, throwOnError: true )!;

    public static readonly System.Type IEnumerableOfNamedService = System.Type.GetType( Name.IEnumerableOfNamedService, throwOnError: true )!;

    [DynamicallyAccessedMembers( PublicConstructors )]
    public static readonly System.Type OpenApiDocumentProvider = System.Type.GetType( Name.OpenApiDocumentProvider, throwOnError: true )!;

    [DynamicallyAccessedMembers( PublicConstructors )]
    public static readonly System.Type OpenApiDocumentService = System.Type.GetType( Name.OpenApiDocumentService, throwOnError: true )!;

    [DynamicallyAccessedMembers( PublicConstructors )]
    public static readonly System.Type OpenApiSchemaService = System.Type.GetType( Name.OpenApiSchemaService, throwOnError: true )!;

    public static class Name
    {
        public const string IDocumentProvider = "Microsoft.Extensions.ApiDescriptions.IDocumentProvider, Microsoft.AspNetCore.OpenApi";
        public const string NamedService = "Microsoft.AspNetCore.OpenApi.NamedService`1[[Microsoft.AspNetCore.OpenApi.OpenApiDocumentService, Microsoft.AspNetCore.OpenApi]], Microsoft.AspNetCore.OpenApi";
        public const string IEnumerableOfNamedService = "System.Collections.Generic.IEnumerable`1[[Microsoft.AspNetCore.OpenApi.NamedService`1[[Microsoft.AspNetCore.OpenApi.OpenApiDocumentService, Microsoft.AspNetCore.OpenApi]], Microsoft.AspNetCore.OpenApi]], System.Private.CoreLib";
        public const string OpenApiDocumentProvider = "Microsoft.Extensions.ApiDescriptions.OpenApiDocumentProvider, Microsoft.AspNetCore.OpenApi";
        public const string OpenApiDocumentService = "Microsoft.AspNetCore.OpenApi.OpenApiDocumentService, Microsoft.AspNetCore.OpenApi";
        public const string OpenApiSchemaService = "Microsoft.AspNetCore.OpenApi.OpenApiSchemaService, Microsoft.AspNetCore.OpenApi";
    }
}