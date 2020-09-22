namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.Extensions.DependencyInjection;
    using System;

    /// <summary>
    /// Represents the provider for creating HTTP error responses matching <see cref="ProblemDetails"/> and RFC 7807.
    /// </summary>
    [CLSCompliant( false )]
    public class ProblemDetailsErrorResponseProvider : IErrorResponseProvider
    {
        /// <inheritdoc />
        public virtual IActionResult CreateResponse( ErrorResponseContext context )
        {
            if ( context == null )
            {
                throw new ArgumentNullException( nameof( context ) );
            }

            return NewResult( context );
        }

        /// <summary>
        /// Creates and returns new problem details.
        /// </summary>
        /// <param name="context">The <see cref="ErrorResponseContext">error context</see> used to generate response.</param>
        /// <returns>New <see cref="ProblemDetails">problem details</see>.</returns>
        protected virtual ProblemDetails NewProblemDetails( ErrorResponseContext context )
        {
            if ( context == null )
            {
                throw new ArgumentNullException( nameof( context ) );
            }

            var httpContext = context.Request.HttpContext;
            var factory = httpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();

            return factory.CreateProblemDetails( httpContext, context.StatusCode, context.ErrorCode, default, context.Message, default );
        }

        /// <summary>
        /// Creates and returns a new result.
        /// </summary>
        /// <param name="context">The <see cref="ErrorResponseContext">error context</see> used to generate response.</param>
        /// <returns>A new <see cref="ObjectResult"/>.</returns>
        protected virtual ObjectResult NewResult( ErrorResponseContext context )
        {
            if ( context == null )
            {
                throw new ArgumentNullException( nameof( context ) );
            }

            // match behavior of IClientErrorFactory.GetClientError
            // REF: https://github.com/dotnet/aspnetcore/blob/master/src/Mvc/Mvc.Core/src/Infrastructure/ProblemDetailsClientErrorFactory.cs#L17
            return new ObjectResult( NewProblemDetails( context ) )
            {
                StatusCode = context.StatusCode,
                ContentTypes =
                {
                    "application/problem+json",
                    "application/problem+xml",
                },
            };
        }
    }
}