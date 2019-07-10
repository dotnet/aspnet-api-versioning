namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Hosting;
    using System;
    using System.Diagnostics.Contracts;
    using static System.String;
#if NETSTANDARD2_0
    using IWebHostEnvironment = Microsoft.Extensions.Hosting.IHostingEnvironment;
#endif

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
        /// <returns>An <see cref="object"/> representing the error content.</returns>
        protected virtual object CreateErrorContent( ErrorResponseContext context )
        {
            Arg.NotNull( context, nameof( context ) );
            Contract.Ensures( Contract.Result<object>() != null );

            return new
            {
                Error = new
                {
                    Code = NullIfEmpty( context.ErrorCode ),
                    Message = NullIfEmpty( context.Message ),
                    InnerError = NewInnerError( context, c => new { Message = c.MessageDetail } ),
                },
            };
        }

        static string NullIfEmpty( string value ) => IsNullOrEmpty( value ) ? null : value;

        static TError NewInnerError<TError>( ErrorResponseContext context, Func<ErrorResponseContext, TError> create )
        {
            Contract.Requires( context != null );
            Contract.Requires( create != null );

            if ( IsNullOrEmpty( context.MessageDetail ) )
            {
                return default;
            }

            var environment = (IWebHostEnvironment) context.Request.HttpContext.RequestServices.GetService( typeof( IWebHostEnvironment ) );

            if ( environment?.IsDevelopment() == true )
            {
                return create( context );
            }

            return default;
        }
    }
}