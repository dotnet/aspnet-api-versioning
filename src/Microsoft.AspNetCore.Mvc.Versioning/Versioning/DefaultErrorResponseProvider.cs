namespace Microsoft.AspNetCore.Mvc.Versioning
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
        /// <param name="context">The <see cref="ErrorResponseContext">error context</see> used to generate the response.</param>
        /// <returns>The generated <see cref="IActionResult">response</see>.</returns>
        public virtual IActionResult CreateResponse( ErrorResponseContext context )
        {
            Arg.NotNull( context, nameof( context ) );
            return new ObjectResult( CreateErrorContent( context ) ) { StatusCode = context.StatusCode };
        }

        /// <summary>
        /// Creates the default error content using the given context.
        /// </summary>
        /// <param name="context">The <see cref="ErrorResponseContext">error context</see> used to create the error content.</param>
        /// <returns>A <see cref="IDictionary{TKey, TValue}">collection</see> of <see cref="KeyValuePair{TKey, TValue}">key/value pairs</see>
        /// representing the error content.</returns>
        protected virtual IDictionary<string, object> CreateErrorContent( ErrorResponseContext context )
        {
            Arg.NotNull( context, nameof( context ) );
            Contract.Ensures( Contract.Result<IDictionary<string, object>>() != null );

            var comparer = StringComparer.OrdinalIgnoreCase;
            var error = new Dictionary<string, object>( comparer );
            var root = new Dictionary<string, object>( comparer ) { ["Error"] = error };

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
                    error["InnerError"] = new Dictionary<string, object>( comparer ) { ["Message"] = context.MessageDetail };
                }
            }

            return root;
        }
    }
}