#pragma warning disable CA1812

namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.Extensions.Primitives;
    using Microsoft.Net.Http.Headers;
    using System;
    using System.Threading.Tasks;
    using static System.StringComparison;
    using static System.Threading.Tasks.Task;

    sealed class ApplyContentTypeVersionActionFilter : IActionFilter
    {
        readonly string parameterName;

        public ApplyContentTypeVersionActionFilter( IApiVersionReader reader ) =>
            parameterName = reader.GetMediaTypeVersionParameter();

        public void OnActionExecuted( ActionExecutedContext context ) { }

        public void OnActionExecuting( ActionExecutingContext context )
        {
            if ( context == null )
            {
                throw new ArgumentNullException( nameof( context ) );
            }

            var httpContext = context.HttpContext;
            var response = httpContext.Response;

            if ( response == null )
            {
                return;
            }

            response.OnStarting( ApplyApiVersionMediaTypeParameter, httpContext );
        }

        Task ApplyApiVersionMediaTypeParameter( object state )
        {
            var context = (HttpContext) state;
            var headers = context.Response.GetTypedHeaders();
            var contentType = headers.ContentType;

            if ( contentType == null )
            {
                return CompletedTask;
            }

            var apiVersion = context.GetRequestedApiVersion();

            if ( apiVersion == null )
            {
                return CompletedTask;
            }

            var parameters = contentType.Parameters;
            var parameter = default( NameValueHeaderValue );

            for ( var i = 0; i < parameters.Count; i++ )
            {
                if ( parameters[i].Name.Equals( parameterName, OrdinalIgnoreCase ) )
                {
                    parameter = parameters[i];
                    break;
                }
            }

            if ( parameter == null )
            {
                parameter = new NameValueHeaderValue( parameterName );
                parameters.Add( parameter );
            }

            parameter.Value = new StringSegment( apiVersion.ToString() );
            headers.ContentType = contentType;
            return CompletedTask;
        }
    }
}