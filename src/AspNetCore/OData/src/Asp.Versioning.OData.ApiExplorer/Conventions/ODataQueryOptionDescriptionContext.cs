// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

using Asp.Versioning.OData;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.OData.Edm;
using static Microsoft.AspNetCore.Http.StatusCodes;

/// <content>
/// Provides additional implementation specific to Microsoft ASP.NET Core.
/// </content>
public partial class ODataQueryOptionDescriptionContext
{
    private static IEdmModel? ResolveModel( ApiDescription description )
    {
        var version = description.GetApiVersion();

        if ( version == null )
        {
            return default;
        }

        var metadata = description.ActionDescriptor.EndpointMetadata;

        if ( metadata == null )
        {
            return default;
        }

        var items = metadata.OfType<IODataRoutingMetadata>();

        foreach ( var item in items )
        {
            var model = item.Model;
            var otherVersion = model.GetApiVersion();

            if ( version.Equals( otherVersion ) )
            {
                return model;
            }
        }

        return default;
    }

    private static bool HasSingleResult( ApiDescription description, out Type? resultType )
    {
        if ( description.SupportedResponseTypes.Count == 0 )
        {
            resultType = default;
            return true;
        }

        var supportedResponseTypes = description.SupportedResponseTypes;
        var candidates = default( List<ApiResponseType> );

        for ( var i = 0; i < supportedResponseTypes.Count; i++ )
        {
            var supportedResponseType = supportedResponseTypes[i];

            if ( supportedResponseType.Type == null )
            {
                continue;
            }

            var statusCode = supportedResponseType.StatusCode;

            if ( statusCode >= Status200OK && statusCode < Status300MultipleChoices )
            {
                candidates ??= new( supportedResponseTypes.Count );
                candidates.Add( supportedResponseType );
            }
        }

        if ( candidates == null || candidates.Count == 0 )
        {
            resultType = default;
            return true;
        }

        candidates.Sort( ( r1, r2 ) => r1.StatusCode.CompareTo( r2.StatusCode ) );

        if ( candidates[0].Type is not Type type )
        {
            resultType = default;
            return false;
        }

        var responseType = type.ExtractInnerType();

        if ( responseType.IsEnumerable( out resultType ) )
        {
            return false;
        }

        resultType = responseType;
        return true;
    }
}