namespace Microsoft.Web.Http.Versioning
{
    using System.Diagnostics.Contracts;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;
    using static System.String;

    /// <summary>
    /// Represents the default implementation for creating HTTP error responses related to API versioning.
    /// </summary>
    public class DefaultErrorResponseProvider : IErrorResponseProvider
    {
        /// <summary>
        /// Creates and returns a new HTTP 400 (Bad Request) given the provided context.
        /// </summary>
        /// <param name="context">The <see cref="ErrorResponseContext">error context</see> used to generate response.</param>
        /// <returns>The generated <see cref="HttpResponseMessage">response</see>.</returns>
        public virtual HttpResponseMessage BadRequest( ErrorResponseContext context )
        {
            Arg.NotNull( context, nameof( context ) );
            return CreateErrorResponse( context, HttpStatusCode.BadRequest );
        }

        /// <summary>
        /// Creates and returns a new HTTP 405 (Method Not Allowed) given the provided context.
        /// </summary>
        /// <param name="context">The <see cref="ErrorResponseContext">error context</see> used to generate response.</param>
        /// <returns>The generated <see cref="HttpResponseMessage">response</see>.</returns>
        public virtual HttpResponseMessage MethodNotAllowed( ErrorResponseContext context )
        {
            Arg.NotNull( context, nameof( context ) );
            return CreateErrorResponse( context, HttpStatusCode.MethodNotAllowed );
        }

        static HttpResponseMessage CreateErrorResponse( ErrorResponseContext context, HttpStatusCode statusCode )
        {
            Contract.Requires( context != null );
            Contract.Ensures( Contract.Result<HttpResponseMessage>() != null );

            var error = IsODataRequest( context ) ? CreateODataError( context ) : CreateWebApiError( context );
            return context.Request.CreateErrorResponse( statusCode, error );
        }

        static HttpResponseMessage CreateWebApiBadRequest( ErrorResponseContext context ) =>
            context.Request.CreateErrorResponse( HttpStatusCode.BadRequest, CreateWebApiError( context ) );

        static HttpResponseMessage CreateODataBadRequest( ErrorResponseContext context ) =>
            context.Request.CreateErrorResponse( HttpStatusCode.BadRequest, CreateODataError( context ) );

        static bool IsODataRequest( ErrorResponseContext context )
        {
            Contract.Requires( context != null );

            var request = context.Request;
            var routeValues = request.GetRouteData();

            if ( routeValues == null )
            {
                return false;
            }

            if ( !routeValues.Values.ContainsKey( "odataPath" ) )
            {
                return false;
            }

            return request.GetConfiguration()?.Formatters.JsonFormatter == null;
        }

        static HttpError CreateWebApiError( ErrorResponseContext context )
        {
            Contract.Requires( context != null );
            Contract.Ensures( Contract.Result<HttpError>() != null );

            var error = new HttpError();
            var root = new HttpError() { ["Error"] = error };

            if ( !IsNullOrEmpty( context.Code ) )
            {
                error["Code"] = context.Code;
            }

            if ( !IsNullOrEmpty( context.Message ) )
            {
                error.Message = context.Message;
            }

            if ( !IsNullOrEmpty( context.MessageDetail ) && context.Request.ShouldIncludeErrorDetail() == true )
            {
                error["InnerError"] = new HttpError( context.MessageDetail );
            }

            return root;
        }

        static HttpError CreateODataError( ErrorResponseContext context )
        {
            Contract.Requires( context != null );
            Contract.Ensures( Contract.Result<HttpError>() != null );

            var error = new HttpError();

            if ( !IsNullOrEmpty( context.Code ) )
            {
                error[HttpErrorKeys.ErrorCodeKey] = context.Code;
            }

            if ( !IsNullOrEmpty( context.Message ) )
            {
                error.Message = context.Message;
            }

            if ( !IsNullOrEmpty( context.MessageDetail ) && context.Request.ShouldIncludeErrorDetail() == true )
            {
                error[HttpErrorKeys.MessageDetailKey] = context.MessageDetail;
            }

            return error;
        }
    }
}