// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OpenApi.Reflection;

using Microsoft.AspNetCore.OpenApi;
using static System.Linq.Expressions.Expression;
using static System.Reflection.BindingFlags;

// HACK: all of these properties are internal in Microsoft.AspNetCore.OpenApi
// REF: https://github.com/dotnet/aspnetcore/tree/main/src/OpenApi/src
internal static class Property
{
    private static readonly Action<OpenApiOptions, string> setDocumentName = NewSetDocumentName();

    extension( OpenApiOptions options )
    {
        public void SetDocumentName( string value ) => setDocumentName( options, value );
    }

    private static Action<OpenApiOptions, string> NewSetDocumentName()
    {
        var options = Parameter( typeof( OpenApiOptions ), "options" );
        var documentName = Parameter( typeof( string ), "documentName" );
        var property = typeof( OpenApiOptions ).GetProperty( nameof( OpenApiOptions.DocumentName ), Instance | NonPublic | Public )!;
        var body = Assign( Property( options, property ), documentName );
        var lambda = Lambda<Action<OpenApiOptions, string>>( body, options, documentName );

        return lambda.Compile();
    }
}