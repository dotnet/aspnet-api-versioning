// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

using Asp.Versioning.OData;
using Microsoft.OData.Edm;
using System.Runtime.CompilerServices;
using System.Web.Http.Description;

/// <content>
/// Provides additional implementation specific to Microsoft ASP.NET Web API.
/// </content>
public partial class ODataQueryOptionDescriptionContext
{
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static IEdmModel? ResolveModel( ApiDescription description ) => description.EdmModel();

    private static bool HasSingleResult( ApiDescription description, out Type? resultType )
    {
        var responseType = description.ResponseDescription.ResponseType;

        if ( responseType == null )
        {
            resultType = default;
            return true;
        }

        responseType = responseType.ExtractInnerType();

        if ( responseType.IsEnumerable( out resultType ) )
        {
            return false;
        }

        resultType = responseType;
        return true;
    }
}