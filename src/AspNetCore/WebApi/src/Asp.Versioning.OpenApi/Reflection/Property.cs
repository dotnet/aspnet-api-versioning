// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OpenApi.Reflection;

using Microsoft.AspNetCore.OpenApi;
using System.Runtime.CompilerServices;

// HACK: all of these properties are internal in Microsoft.AspNetCore.OpenApi
// REF: https://github.com/dotnet/aspnetcore/tree/main/src/OpenApi/src
internal static class Property
{
    extension( OpenApiOptions options )
    {
        public void SetDocumentName( string value ) => SetDocumentNameImpl( options, value );
    }

    [UnsafeAccessor( UnsafeAccessorKind.Method, Name = "set_DocumentName" )]
    private static extern void SetDocumentNameImpl( OpenApiOptions options, string value );
}