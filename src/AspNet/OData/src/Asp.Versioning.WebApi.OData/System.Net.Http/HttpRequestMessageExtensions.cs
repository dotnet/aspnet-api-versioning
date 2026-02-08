// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace System.Net.Http;

using Asp.Versioning;
using Microsoft.OData;
using System.Web.Http;
using static System.Net.HttpStatusCode;

internal static class HttpRequestMessageExtensions
{
    extension( HttpRequestMessage request )
    {
        internal ApiVersion? GetRequestedApiVersionOrReturnBadRequest()
        {
            var properties = request.ApiVersionProperties;

            if ( properties.RawRequestedApiVersions.Count < 2 )
            {
                return properties.RequestedApiVersion;
            }

            var error = new ODataError()
            {
                ErrorCode = ProblemDetailsDefaults.Ambiguous.Code,
                Message = new AmbiguousApiVersionException().Message,
            };

            throw new HttpResponseException( request.CreateResponse( BadRequest, error ) );
        }
    }
}