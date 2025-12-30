// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace Microsoft.AspNetCore.Mvc.ApiExplorer;

internal static class ApiDescriptionExtensions
{
    internal static bool IsODataLike( this ApiDescription description )
    {
        var parameters = description.ActionDescriptor.Parameters;

        for ( var i = 0; i < parameters.Count; i++ )
        {
            if ( parameters[i].ParameterType.IsODataQueryOptions() )
            {
                return true;
            }
        }

        return false;
    }
}