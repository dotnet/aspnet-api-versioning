// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OpenApi.Reflection;

using static System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes;

// HACK: all of these types are internal in Microsoft.AspNetCore.OpenApi
// REF: https://github.com/dotnet/aspnetcore/tree/main/src/OpenApi/src
internal static class Type
{
    [DynamicallyAccessedMembers( PublicConstructors )]
    public static readonly System.Type IDocumentProvider = System.Type.GetType( "Microsoft.Extensions.ApiDescriptions.IDocumentProvider, Microsoft.AspNetCore.OpenApi", throwOnError: true )!;

    [DynamicallyAccessedMembers( PublicConstructors )]
    public static readonly System.Type NamedService = System.Type.GetType( "Microsoft.AspNetCore.OpenApi.NamedService`1[[Microsoft.AspNetCore.OpenApi.OpenApiDocumentService, Microsoft.AspNetCore.OpenApi]], Microsoft.AspNetCore.OpenApi", throwOnError: true )!;

    public static readonly System.Type IEnumerableOfNamedService = System.Type.GetType( "System.Collections.Generic.IEnumerable`1[[Microsoft.AspNetCore.OpenApi.NamedService`1[[Microsoft.AspNetCore.OpenApi.OpenApiDocumentService, Microsoft.AspNetCore.OpenApi]], Microsoft.AspNetCore.OpenApi]], System.Private.CoreLib", throwOnError: true )!;

    public static readonly System.Type ListOfNamedService = System.Type.GetType( "System.Collections.Generic.List`1[[Microsoft.AspNetCore.OpenApi.NamedService`1[[Microsoft.AspNetCore.OpenApi.OpenApiDocumentService, Microsoft.AspNetCore.OpenApi]], Microsoft.AspNetCore.OpenApi]], System.Private.CoreLib", throwOnError: true )!;

    [DynamicallyAccessedMembers( PublicConstructors )]
    public static readonly System.Type OpenApiDocumentProvider = System.Type.GetType( "Microsoft.Extensions.ApiDescriptions.OpenApiDocumentProvider, Microsoft.AspNetCore.OpenApi", throwOnError: true )!;

    [DynamicallyAccessedMembers( PublicConstructors )]
    public static readonly System.Type OpenApiDocumentService = System.Type.GetType( "Microsoft.AspNetCore.OpenApi.OpenApiDocumentService, Microsoft.AspNetCore.OpenApi", throwOnError: true )!;

    [DynamicallyAccessedMembers( PublicConstructors )]
    public static readonly System.Type OpenApiSchemaService = System.Type.GetType( "Microsoft.AspNetCore.OpenApi.OpenApiSchemaService, Microsoft.AspNetCore.OpenApi", throwOnError: true )!;

    [DynamicallyAccessedMembers( PublicConstructors )]
    public static readonly System.Type OpenApiSchemaJsonOptions = System.Type.GetType( "Microsoft.AspNetCore.OpenApi.OpenApiSchemaJsonOptions, Microsoft.AspNetCore.OpenApi", throwOnError: true )!;
}