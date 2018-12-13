namespace Microsoft.Examples
{
    using System.Linq;
    using System.Web.Http;
    using System.Web.Http.Results;
    using static System.Net.HttpStatusCode;

    /// <summary>
    /// Provides extension methods for <see cref="ApiController"/>.
    /// </summary>
    public static class ApiControllerExtensions
    {
        /// <summary>
        /// Returns HTTP status code 200 (OK) for the specified query results.
        /// </summary>
        /// <param name="controller">The extended <see cref="ApiController">controller</see>.</param>
        /// <param name="results">The <see cref="IQueryable">query</see> results.</param>
        /// <returns>The <see cref="IHttpActionResult">response</see> for the specified <paramref name="results"/>.</returns>
        /// <remarks>This extension method addresses a known issue where the <see cref="IQueryable"/> results may not
        /// correctly negotiate the entity model and media type formatter.</remarks>
        public static IHttpActionResult Success( this ApiController controller, IQueryable results ) =>
            new ResponseMessageResult( controller.Request.CreateResponse( OK, results.GetType(), results ) );

        /// <summary>
        /// Returns HTTP status code 200 (OK) or 404 (Not Found) for the specified result.
        /// </summary>
        /// <param name="controller">The extended <see cref="ApiController">controller</see> .</param>
        /// <param name="result">The resultant object.</param>
        /// <returns>The <see cref="IHttpActionResult">response</see> for the specified <paramref name="result"/>
        /// If the <paramref name="result"/> is <c>null</c>, HTTP status code 404 (Not Found) is returned;
        /// otherwise, HTTP status code 200 (OK) is returned.</returns>
        /// <remarks>This extension method addresses a known issue where the <see cref="IQueryable"/> results may not
        /// correctly negotiate the entity model and media type formatter.</remarks>
        public static IHttpActionResult SuccessOrNotFound( this ApiController controller, object result )
        {
            if ( result == null )
            {
                return new NotFoundResult( controller );
            }

            return new ResponseMessageResult( controller.Request.CreateResponse( OK, result.GetType(), result ) );
        }
    }
}