﻿namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Hosting;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using static System.String;

    /// <summary>
    /// Represents the default implementation for creating HTTP error responses related to API versioning.
    /// </summary>
    [CLSCompliant( false )]
    public class DefaultErrorResponseProvider : IErrorResponseProvider
    {
        /// <summary>
        /// Creates and returns a new error response given the provided context.
        /// </summary>
        /// <param name="context">The <see cref="ErrorResponseContext">error context</see> used to generate response.</param>
        /// <returns>The generated <see cref="IActionResult">response</see>.</returns>
        public virtual IActionResult CreateResponse( ErrorResponseContext context )
        {
            Arg.NotNull( context, nameof( context ) );
            return new ObjectResult( CreateErrorContent( context ) ) { StatusCode = context.StatusCode };
        }

        static object CreateErrorContent( ErrorResponseContext context )
        {
            Contract.Requires( context != null );
            Contract.Ensures( Contract.Result<object>() != null );

            var error = new Dictionary<string, object>();
            var root = new Dictionary<string, object>() { ["Error"] = error };

            if ( !IsNullOrEmpty( context.ErrorCode ) )
            {
                error["Code"] = context.ErrorCode;
            }

            if ( !IsNullOrEmpty( context.Message ) )
            {
                error["Message"] = context.Message;
            }

            if ( !IsNullOrEmpty( context.MessageDetail ) )
            {
                var environment = (IHostingEnvironment) context.Request.HttpContext.RequestServices.GetService( typeof( IHostingEnvironment ) );

                if ( environment?.IsDevelopment() == true )
                {
                    error["InnerError"] = new Dictionary<string, object>() { ["Message"] = context.MessageDetail };
                }
            }

            return root;
        }
    }
}