namespace Microsoft.Web.Http.Versioning
{
    using System.Diagnostics.Contracts;
    using System.Net.Http;
    using System.Net.Http.Formatting;
    using System.Web.Http;
    using static System.Net.HttpStatusCode;
    using static System.String;

    /// <content>
    /// Provides additional implementation specific to Microsoft ASP.NET Web API.
    /// </content>
    public partial class ApiVersioningOptions
    {
        private CreateBadRequestDelegate createBadRequest = CreateDefaultBadRequest;

        /// <summary>
        /// Gets or sets the function to used to create HTTP 400 (Bad Request) responses related to API versioning.
        /// </summary>
        /// <value>The <see cref="CreateBadRequestDelegate">function</see> to used to create a HTTP 400 (Bad Request)
        /// <see cref="HttpResponseMessage">response</see> related to API versioning.</value>
        /// <remarks>The default value generates responses that are compliant with the Microsoft REST API Guidelines.
        /// This option should only be changed by service authors that intentionally want to deviate from the
        /// established guidance.</remarks>
        public CreateBadRequestDelegate CreateBadRequest
        {
            get
            {
                Contract.Ensures( createBadRequest != null );
                return createBadRequest;
            }
            set
            {
                Arg.NotNull( value, nameof( value ) );
                createBadRequest = value;
            }
        }

        private static HttpResponseMessage CreateDefaultBadRequest( HttpRequestMessage request, string code, string message, string messageDetail )
        {
            if ( request == null || !IsODataRequest( request ) )
            {
                return CreateWebApiBadRequest( request, code, message, messageDetail );
            }

            return CreateODataBadRequest( request, code, message, messageDetail );
        }

        private static HttpResponseMessage CreateWebApiBadRequest( HttpRequestMessage request, string code, string message, string messageDetail )
        {
            var error = new HttpError();
            var root = new HttpError() { ["Error"] = error };

            if ( !IsNullOrEmpty( code ) )
            {
                error["Code"] = code;
            }

            if ( !IsNullOrEmpty( message ) )
            {
                error.Message = message;
            }

            if ( !IsNullOrEmpty( messageDetail ) && request?.ShouldIncludeErrorDetail() == true )
            {
                error["InnerError"] = new HttpError( messageDetail );
            }

            if ( request == null )
            {
                return new HttpResponseMessage( BadRequest )
                {
                    Content = new ObjectContent<HttpError>( root, new JsonMediaTypeFormatter() )
                };
            }

            return request.CreateErrorResponse( BadRequest, root );
        }

        private static bool IsODataRequest( HttpRequestMessage request )
        {
            if ( request == null )
            {
                return false;
            }

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

        private static HttpResponseMessage CreateODataBadRequest( HttpRequestMessage request, string code, string message, string messageDetail )
        {
            Contract.Requires( request != null );

            var error = new HttpError();

            if ( !IsNullOrEmpty( code ) )
            {
                error[HttpErrorKeys.ErrorCodeKey] = code;
            }

            if ( !IsNullOrEmpty( message ) )
            {
                error.Message = message;
            }

            if ( !IsNullOrEmpty( messageDetail ) && request?.ShouldIncludeErrorDetail() == true )
            {
                error[HttpErrorKeys.MessageDetailKey] = messageDetail;
            }

            return request.CreateErrorResponse( BadRequest, error );
        }
    }
}