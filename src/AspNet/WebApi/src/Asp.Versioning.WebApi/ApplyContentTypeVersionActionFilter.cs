// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using System.Net.Http.Headers;
using System.Web.Http.Filters;
using static Asp.Versioning.ApiVersionParameterLocation;

internal sealed class ApplyContentTypeVersionActionFilter : ActionFilterAttribute
{
    private readonly string parameterName;

    internal ApplyContentTypeVersionActionFilter( IApiVersionReader reader ) =>
        parameterName = reader.GetParameterName( MediaTypeParameter );

    public override bool AllowMultiple => false;

    public override void OnActionExecuted( HttpActionExecutedContext actionExecutedContext )
    {
        var response = actionExecutedContext.Response;

        if ( response == null )
        {
            return;
        }

        var headers = response.Content?.Headers;
        var contentType = headers?.ContentType;

        if ( contentType == null )
        {
            return;
        }

        var apiVersion = actionExecutedContext.Request.GetRequestedApiVersion();

        if ( apiVersion == null )
        {
            return;
        }

        var parameters = contentType.Parameters;
        var versionParameter = default( NameValueHeaderValue );
        var comparer = StringComparer.OrdinalIgnoreCase;

        foreach ( var parameter in parameters )
        {
            if ( comparer.Equals( parameter.Name, parameterName ) )
            {
                versionParameter = parameter;
                break;
            }
        }

        if ( versionParameter == null )
        {
            versionParameter = new( parameterName );
            parameters.Add( versionParameter );
        }

        versionParameter.Value = apiVersion.ToString();
        headers!.ContentType = contentType;
    }
}