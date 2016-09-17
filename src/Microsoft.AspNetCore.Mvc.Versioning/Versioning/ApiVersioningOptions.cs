namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Hosting;
    using Http;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
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
        /// <see cref="HttpResponse">response</see> related to API versioning.</value>
        /// <remarks>The default value generates responses that are compliant with the Microsoft REST API Guidelines.
        /// This option should only be changed by service authors that intentionally want to deviate from the
        /// established guidance.</remarks>
        [CLSCompliant( false )]
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

        private static BadRequestObjectResult CreateDefaultBadRequest( HttpRequest request, string code, string message, string messageDetail )
        {
            var error = new Dictionary<string, object>();
            var root = new Dictionary<string, object>() { ["Error"] = error };

            if ( !IsNullOrEmpty( code ) )
            {
                error["Code"] = code;
            }

            if ( !IsNullOrEmpty( message ) )
            {
                error["Message"] = message;
            }

            if ( !IsNullOrEmpty( messageDetail ) )
            {
                var environment = (IHostingEnvironment) request?.HttpContext.RequestServices.GetService( typeof( IHostingEnvironment ) );

                if ( environment?.IsDevelopment() == true )
                {
                    error["InnerError"] = new Dictionary<string, object>() { ["Message"] = messageDetail };
                }
            }

            return new BadRequestObjectResult( root );
        }
    }
}