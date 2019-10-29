namespace Microsoft.Web.Http.Versioning
{
    using System;
    using System.Net.Http;
    using System.Web.Http;
    using static System.String;

    /// <summary>
    /// Represents the default implementation for creating HTTP error responses related to API versioning.
    /// </summary>
    public class DefaultErrorResponseProvider : IErrorResponseProvider
    {
        /// <summary>
        /// Creates and returns a new error response given the provided context.
        /// </summary>
        /// <param name="context">The <see cref="ErrorResponseContext">error context</see> used to generate response.</param>
        /// <returns>The generated <see cref="HttpResponseMessage">response</see>.</returns>
        public virtual HttpResponseMessage CreateResponse( ErrorResponseContext context )
        {
            if ( context == null )
            {
                throw new ArgumentNullException( nameof( context ) );
            }

            return context.Request.CreateErrorResponse( context.StatusCode, CreateErrorContent( context ) );
        }

        /// <summary>
        /// Creates the default error content using the given context.
        /// </summary>
        /// <param name="context">The <see cref="ErrorResponseContext">error context</see> used to create the error content.</param>
        /// <returns>A <see cref="HttpError">HTTP error</see> representing the error content.</returns>
        protected virtual HttpError CreateErrorContent( ErrorResponseContext context ) =>
            IsODataRequest( context ) ? CreateODataError( context ) : CreateWebApiError( context );

        static bool IsODataRequest( ErrorResponseContext context )
        {
            if ( context == null )
            {
                throw new ArgumentNullException( nameof( context ) );
            }

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
            var error = new HttpError();
            var root = new HttpError() { ["Error"] = error };

            if ( !IsNullOrEmpty( context.ErrorCode ) )
            {
                error["Code"] = context.ErrorCode;
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
            var error = new HttpError();

            if ( !IsNullOrEmpty( context.ErrorCode ) )
            {
                error[HttpErrorKeys.ErrorCodeKey] = context.ErrorCode;
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